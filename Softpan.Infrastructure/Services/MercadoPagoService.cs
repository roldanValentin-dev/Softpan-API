using MercadoPago.Client.Preference;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Softpan.Application.DTOs;
using Softpan.Application.Exceptions;
using Softpan.Application.Interfaces;
using Softpan.Domain.Enums;
using Softpan.Domain.Interfaces;
using Softpan.Infrastructure.Data;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using static Softpan.Application.DTOs.MercadoPagoDto;

namespace Softpan.Infrastructure.Services;

public class MercadoPagoService(
    IPedidoRepository pedidoRepository,
    IProductoRepository productoRepository,
    ApplicationDbContext context,
    IConfiguration configuration,
    IHttpClientFactory httpClientFactory) : IMercadoPagoService
{
    // ========================================================================
    // SEGURIDAD 1: Credenciales solo desde variables de entorno
    // ========================================================================
    // NUNCA colocar el Access Token en appsettings.json o código fuente.
    // Configurarlo como variable de entorno del servidor:
    //   Linux:  export MercadoPago__AccessToken="APP_USR-..."
    //   Docker: environment: MercadoPago__AccessToken: "APP_USR-..."
    //   Azure:  Application Settings > MercadoPago:AccessToken
    private readonly string _accessToken = configuration["MercadoPago:AccessToken"]
        ?? throw new InvalidOperationException(
            "MercadoPago:AccessToken no configurado. " +
            "Configurar como variable de entorno MercadoPago__AccessToken");

    // ========================================================================
    // SEGURIDAD 2: Client Secret para validar firma de webhooks
    // ========================================================================
    // Se usa para verificar que las notificaciones fueron enviadas por MP
    // y no por un atacante. Configurar como variable de entorno:
    //   MercadoPago__ClientSecret="tu-secret"
    private readonly string? _clientSecret = configuration["MercadoPago:ClientSecret"];

    // ========================================================================
    // SEGURIDAD 3: HttpClientFactory en vez de new HttpClient()
    // ========================================================================
    // new HttpClient() agota los sockets del servidor y no permite
    // configurar timeouts, retry policies ni headers globales.
    // IHttpClientFactory reusa sockets, mejora performance y seguridad.
    // Configuración del cliente en DependencyInjections.cs
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;

    // ========================================================================
    // CREAR PREFERENCIA DE PAGO
    // ========================================================================
    public async Task<MercadoPagoPreferenceResponseDto> CrearPreferenciaPagoAsync(int pedidoId, string? emailPagador)
    {
        var pedido = await pedidoRepository.GetByIdWithDetallesAsync(pedidoId);
        if (pedido == null)
            throw new NotFoundException("Pedido", pedidoId);

        if (pedido.Estado != EstadoPedidoEnum.Pendiente)
            throw new BadRequestException("El pedido debe estar en estado Pendiente");

        // ====================================================================
        // SEGURIDAD 4: Validar stock y precios desde BD
        // ====================================================================
        // NUNCA confiar en precios, cantidades o productos enviados desde el
        // frontend. Un atacante podría modificar el JavaScript y enviar datos
        // fraudulentos. Siempre leer los precios reales desde la base de datos.
        var items = new List<PreferenceItemRequest>();
        foreach (var detalle in pedido.Detalles)
        {
            var producto = await productoRepository.GetByIdAsync(detalle.ProductoId);
            if (producto == null || !producto.Activo)
                throw new BadRequestException($"Producto {detalle.ProductoId} no disponible");
            if (!producto.TieneStock(detalle.Cantidad))
                throw new BadRequestException($"Stock insuficiente para {producto.Nombre}");

            items.Add(new PreferenceItemRequest
            {
                Title = producto.Nombre,
                Quantity = detalle.Cantidad,
                CurrencyId = "ARS",
                UnitPrice = detalle.PrecioUnitario
            });
        }

        var baseUrl = configuration["MercadoPago:BaseUrl"] ?? "http://localhost:5173";
        var notifUrl = configuration["MercadoPago:NotificationUrl"] ?? $"{baseUrl}/api/mercadopago/webhook";

        var payload = new
        {
            items = items.Select(i => new
            {
                title = i.Title,
                quantity = i.Quantity,
                currency_id = i.CurrencyId,
                unit_price = i.UnitPrice
            }),
            external_reference = pedido.Id.ToString(),
            notification_url = notifUrl,
            back_urls = new
            {
                success = $"{baseUrl}/pago-exitoso",
                failure = $"{baseUrl}/pago-fallido",
                pending = $"{baseUrl}/pago-pendiente"
            },
            auto_return = "approved",
            payer = string.IsNullOrEmpty(emailPagador)
                ? null
                : new { email = emailPagador }
        };

        using var http = _httpClientFactory.CreateClient("MercadoPago");
        var json = JsonSerializer.Serialize(payload, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
        });
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        http.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", _accessToken);

        var response = await http.PostAsync("https://api.mercadopago.com/checkout/preferences", content);
        var responseBody = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
            throw new Exception($"Error de Mercado Pago: {responseBody}");

        using var doc = JsonDocument.Parse(responseBody);
        var root = doc.RootElement;
        var preferenceId = root.GetProperty("id").GetString()!;
        var initPoint = root.GetProperty("init_point").GetString();

        pedido.MercadoPagoPreferenceId = preferenceId;
        pedido.PaymentGateway = "mercadopago";
        await pedidoRepository.UpdateAsync(pedido);

        return new MercadoPagoPreferenceResponseDto
        {
            PreferenceId = preferenceId,
            InitPoint = initPoint!,
            PedidoId = pedido.Id
        };
    }

    // ========================================================================
    // PROCESAR WEBHOOK
    // ========================================================================
    public async Task<PagoResultadoDto> ProcesarWebhookMercadoPagoAsync(string webhookJson, string xSignatureHeader, string xRequestId)
    {
        // ====================================================================
        // SEGURIDAD 5: Parsear notificación
        // ====================================================================
        using var doc = System.Text.Json.JsonDocument.Parse(webhookJson);
        var root = doc.RootElement;

        var type = root.TryGetProperty("type", out var t) ? t.GetString() : null;
        if (type != "payment")
            return new PagoResultadoDto { Exitoso = false, Mensaje = "Tipo de notificación no soportado" };

        var dataId = root.TryGetProperty("data", out var data)
            && data.TryGetProperty("id", out var id)
            ? id.GetString()
            : null;

        if (string.IsNullOrEmpty(dataId))
            return new PagoResultadoDto { Exitoso = false, Mensaje = "ID de pago no encontrado en notificación" };

        // ====================================================================
        // SEGURIDAD 6: Validar firma HMAC del webhook
        // ====================================================================
        // Mercado Pago envía un header X-Signature con el formato:
        //   ts=1712345678,v1=abcdef123456...
        // Donde v1 = HMAC-SHA256(id_pago + request_id + timestamp)
        //
        // Si la firma no coincide, RECHAZAMOS la notificación.
        // Esto evita que un atacante envíe webhooks falsos.
        if (!string.IsNullOrEmpty(_clientSecret))
        {
            var esFirmaValida = ValidarFirmaWebhook(xSignatureHeader, dataId, xRequestId);
            if (!esFirmaValida)
                return new PagoResultadoDto
                {
                    Exitoso = false,
                    Mensaje = "Firma de webhook inválida"
                };
        }

        // ====================================================================
        // SEGURIDAD 7: Consultar estado REAL en API de MP
        // ====================================================================
        // NO confiar en el estado que viene en el JSON del webhook.
        // Un atacante podría enviar un webhook falso con status=approved.
        // Siempre consultar la API oficial de MP para obtener el estado real.
        var httpClient = _httpClientFactory.CreateClient("MercadoPago");
        httpClient.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _accessToken);

        using var response = await httpClient.GetAsync($"https://api.mercadopago.com/v1/payments/{dataId}");
        if (!response.IsSuccessStatusCode)
            return new PagoResultadoDto { Exitoso = false, Mensaje = "Error al consultar pago en Mercado Pago" };

        using var paymentStream = await response.Content.ReadAsStreamAsync();
        using var paymentDoc = System.Text.Json.JsonDocument.Parse(paymentStream);
        var payment = paymentDoc.RootElement;

        var status = payment.TryGetProperty("status", out var s) ? s.GetString() : null;
        var externalRef = payment.TryGetProperty("external_reference", out var er) ? er.GetString() : null;
        var statusDetail = payment.TryGetProperty("status_detail", out var sd) ? sd.GetString() : null;

        if (!int.TryParse(externalRef, out var pedidoId))
            return new PagoResultadoDto { Exitoso = false, Mensaje = "external_reference inválido" };

        // ====================================================================
        // SEGURIDAD 8: Transacción de base de datos
        // ====================================================================
        // Si el proceso falla a mitad de camino (ej: se descuenta stock pero
        // falla al guardar el pedido), la transacción hace ROLLBACK de TODO.
        // Esto garantiza que NUNCA hay inconsistencia entre stock y pedidos.
        using var transaction = await context.Database.BeginTransactionAsync();

        try
        {
            var pedido = await pedidoRepository.GetByIdWithDetallesAsync(pedidoId);
            if (pedido == null)
                return new PagoResultadoDto { Exitoso = false, Mensaje = "Pedido no encontrado" };

            // Idempotencia: si ya procesamos este paymentId, ignoramos
            if (pedido.MercadoPagoPaymentId == dataId)
                return new PagoResultadoDto { Exitoso = true, Mensaje = "Webhook duplicado ignorado" };

            pedido.MercadoPagoPaymentId = dataId;
            pedido.PaymentStatus = status;
            pedido.PaymentStatusDetails = statusDetail;
            pedido.PaymentFechaActualizado = DateTime.UtcNow;

            switch (status)
            {
                case "approved":
                    pedido.EstadoPago = EstadoPagoEnum.Pagado;
                    pedido.FechaPago = DateTime.UtcNow;
                    pedido.Estado = EstadoPedidoEnum.Confirmado;
                    pedido.StockDescontado = true;
                    foreach (var detalle in pedido.Detalles)
                    {
                        detalle.Producto?.DescontarStock(detalle.Cantidad);
                    }
                    break;

                case "rejected":
                case "cancelled":
                    pedido.EstadoPago = EstadoPagoEnum.Fallido;
                    break;

                case "refunded":
                    pedido.EstadoPago = EstadoPagoEnum.Reembolsado;
                    break;
            }

            await pedidoRepository.UpdateAsync(pedido);
            await transaction.CommitAsync();

            return new PagoResultadoDto
            {
                Exitoso = true,
                Mensaje = $"Pago {status} procesado",
                PedidoId = pedido.Id,
                PaymentId = dataId
            };
        }
        catch (Exception)
        {
            // ================================================================
            // SEGURIDAD 9: Rollback automático si algo falla
            // ================================================================
            // Si hay cualquier error (BD caída, excepción en descuento de
            // stock, etc.), se deshacen TODOS los cambios pendientes.
            // El sistema queda exactamente como antes del webhook.
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<EstadoPagoDto> ConsultarEstadoPagoAsync(string preferenceId)
    {
        var pedido = await context.Pedidos
            .FirstOrDefaultAsync(p => p.MercadoPagoPreferenceId == preferenceId);

        if (pedido == null)
            return new EstadoPagoDto { Estado = "no_encontrado" };

        return new EstadoPagoDto
        {
            Estado = pedido.PaymentStatus ?? pedido.EstadoPago.ToString(),
            Detalle = pedido.PaymentStatusDetails,
            FechaActualizacion = pedido.PaymentFechaActualizado
        };
    }

    // ========================================================================
    // SEGURIDAD 10: Validación de firma HMAC-SHA256
    // ========================================================================
    // Algoritmo:
    //   1. Extraer timestamp (ts) y hash (v1) del header X-Signature
    //   2. Armar el template: "id:{dataId};request-id:{requestId};ts:{ts};"
    //   3. Computar HMAC-SHA256 del template usando el ClientSecret
    //   4. Comparar con v1 (en lowercase)
    //
    // Si no coincide → la notificación NO fue enviada por Mercado Pago
    private bool ValidarFirmaWebhook(string xSignatureHeader, string dataId, string xRequestId)
    {
        if (string.IsNullOrEmpty(xSignatureHeader) || string.IsNullOrEmpty(_clientSecret))
            return false;

        // Parsear el header: "ts=1712345678,v1=abcdef123456..."
        var partes = xSignatureHeader.Split(',');
        var ts = string.Empty;
        var v1 = string.Empty;

        foreach (var parte in partes)
        {
            var kv = parte.Split('=', 2);
            if (kv.Length == 2)
            {
                if (kv[0].Trim() == "ts") ts = kv[1].Trim();
                if (kv[0].Trim() == "v1") v1 = kv[1].Trim();
            }
        }

        if (string.IsNullOrEmpty(ts) || string.IsNullOrEmpty(v1))
            return false;

        // Armar el template para HMAC: "id:{dataId};request-id:{requestId};ts:{ts};"
        // requestId se extrae del header X-Request-Id enviado por MP.
        // Si no viene, usamos dataId como fallback (no es ideal, pero es mejor que nada).
        var requestId = !string.IsNullOrEmpty(xRequestId) ? xRequestId : dataId;
        var template = $"id:{dataId};request-id:{requestId};ts:{ts};";

        // Computar HMAC-SHA256
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_clientSecret));
        var hashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(template));
        var hashEsperado = Convert.ToHexString(hashBytes).ToLower();

        return hashEsperado == v1.ToLower();
    }
}
