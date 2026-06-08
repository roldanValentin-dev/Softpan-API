using Mapster;
using Softpan.Application.DTOs;
using Softpan.Application.Exceptions;
using Softpan.Application.Interfaces;
using Softpan.Domain.Entities;
using Softpan.Domain.Enums;
using Softpan.Domain.Interfaces;

namespace Softpan.Application.Services;

public class AdminPagoService(
    IConfiguracionRepository configuracionRepo,
    IDatosBancariosRepository datosBancariosRepo,
    IDireccionRetiroRepository direccionRetiroRepo,
    IPedidoRepository pedidoRepository) : IAdminPagoService
{
    private const string CLAVE_DESCUENTO = "DescuentoEfectivoTransferencia";
    private const string CLAVE_COSTO_ENVIO = "CostoEnvio";
    private const string CLAVE_MINIMO_GRATIS = "CostoEnvioMinimoGratis";

    // ========================================================================
    // DESCUENTO
    // ========================================================================
    public async Task<ConfiguracionPagoDto> GetDescuentoAsync()
    {
        var config = await configuracionRepo.GetByClaveAsync(CLAVE_DESCUENTO);
        if (config == null)
            return new ConfiguracionPagoDto { Clave = CLAVE_DESCUENTO, Valor = "10", Descripcion = "Descuento para Efectivo/Transferencia (%)" };

        return config.Adapt<ConfiguracionPagoDto>();
    }

    public async Task<ConfiguracionPagoDto> UpdateDescuentoAsync(decimal porcentaje)
    {
        if (porcentaje < 0 || porcentaje > 100)
            throw new BadRequestException("El descuento debe estar entre 0 y 100");

        var config = await configuracionRepo.GetByClaveAsync(CLAVE_DESCUENTO);
        if (config == null)
        {
            config = new ConfiguracionPago
            {
                Clave = CLAVE_DESCUENTO,
                Valor = porcentaje.ToString(),
                Descripcion = "Descuento para Efectivo/Transferencia (%)"
            };
            // Crear nuevo registro si no existe
            var created = await configuracionRepo.CreateAsync(config);
            return created.Adapt<ConfiguracionPagoDto>();
        }

        config.Valor = porcentaje.ToString();
        var updated = await configuracionRepo.UpdateAsync(config);
        return updated.Adapt<ConfiguracionPagoDto>();
    }

    // ========================================================================
    // DATOS BANCARIOS
    // ========================================================================
    public async Task<List<DatosBancariosDto>> GetDatosBancariosAsync()
    {
        var datos = await datosBancariosRepo.GetAllAsync();
        return datos.Select(d => d.Adapt<DatosBancariosDto>()).ToList();
    }

    public async Task<DatosBancariosDto> CreateDatosBancariosAsync(CreateDatosBancariosDto dto)
    {
        var datos = dto.Adapt<DatosBancarios>();
        // Si es el primero, activarlo automáticamente
        var existentes = await datosBancariosRepo.GetAllAsync();
        if (!existentes.Any())
            datos.Activo = true;

        var created = await datosBancariosRepo.CreateAsync(datos);
        return created.Adapt<DatosBancariosDto>();
    }

    public async Task<DatosBancariosDto> UpdateDatosBancariosAsync(int id, UpdateDatosBancariosDto dto)
    {
        var existente = await datosBancariosRepo.GetByIdAsync(id);
        if (existente == null)
            throw new NotFoundException("Datos bancarios", id);

        dto.Adapt(existente);
        var updated = await datosBancariosRepo.UpdateAsync(existente);
        return updated.Adapt<DatosBancariosDto>();
    }

    public async Task<bool> DeleteDatosBancariosAsync(int id)
    {
        return await datosBancariosRepo.DeleteAsync(id);
    }

    // ========================================================================
    // DIRECCIÓN DE RETIRO
    // ========================================================================
    public async Task<DireccionRetiroDto> GetDireccionRetiroAsync()
    {
        var direccion = await direccionRetiroRepo.GetActivaAsync();
        if (direccion == null)
            return new DireccionRetiroDto();

        return direccion.Adapt<DireccionRetiroDto>();
    }

    public async Task<DireccionRetiroDto> UpdateDireccionRetiroAsync(UpdateDireccionRetiroDto dto)
    {
        var existente = await direccionRetiroRepo.GetActivaAsync();
        if (existente == null)
        {
            var nueva = dto.Adapt<DireccionRetiro>();
            nueva.Activo = true;
            var created = await direccionRetiroRepo.CreateAsync(nueva);
            return created.Adapt<DireccionRetiroDto>();
        }

        dto.Adapt(existente);
        var updated = await direccionRetiroRepo.UpdateAsync(existente);
        return updated.Adapt<DireccionRetiroDto>();
    }

    // ========================================================================
    // COSTO DE ENVÍO
    // ========================================================================
    public async Task<CostoEnvioConfigDto> GetCostoEnvioConfigAsync()
    {
        var costo = (await configuracionRepo.GetByClaveAsync(CLAVE_COSTO_ENVIO))?.Valor;
        var minimo = (await configuracionRepo.GetByClaveAsync(CLAVE_MINIMO_GRATIS))?.Valor;

        return new CostoEnvioConfigDto
        {
            CostoEnvio = decimal.TryParse(costo, out var c) ? c : 0,
            MinimoGratis = decimal.TryParse(minimo, out var m) ? m : null
        };
    }

    public async Task<CostoEnvioConfigDto> UpdateCostoEnvioConfigAsync(CostoEnvioConfigDto dto)
    {
        if (dto.CostoEnvio < 0)
            throw new BadRequestException("El costo de envío no puede ser negativo");
        if (dto.MinimoGratis < 0)
            throw new BadRequestException("El mínimo para envío gratis no puede ser negativo");

        var configCosto = await configuracionRepo.GetByClaveAsync(CLAVE_COSTO_ENVIO);
        if (configCosto == null)
        {
            configCosto = new ConfiguracionPago
            {
                Clave = CLAVE_COSTO_ENVIO,
                Valor = dto.CostoEnvio.ToString(),
                Descripcion = "Costo fijo de envío"
            };
            await configuracionRepo.CreateAsync(configCosto);
        }
        else
        {
            configCosto.Valor = dto.CostoEnvio.ToString();
            await configuracionRepo.UpdateAsync(configCosto);
        }

        var configMinimo = await configuracionRepo.GetByClaveAsync(CLAVE_MINIMO_GRATIS);
        if (configMinimo == null)
        {
            configMinimo = new ConfiguracionPago
            {
                Clave = CLAVE_MINIMO_GRATIS,
                Valor = dto.MinimoGratis?.ToString() ?? "",
                Descripcion = "Monto mínimo para envío gratis"
            };
            await configuracionRepo.CreateAsync(configMinimo);
        }
        else
        {
            configMinimo.Valor = dto.MinimoGratis?.ToString() ?? "";
            await configuracionRepo.UpdateAsync(configMinimo);
        }

        return dto;
    }

    // ========================================================================
    // PEDIDOS PENDIENTES DE PAGO
    // ========================================================================
    public async Task<List<PedidoPendientePagoDto>> GetPedidosPendientesPagoAsync()
    {
        var pedidos = await pedidoRepository.GetByEstadoAsync(EstadoPedidoEnum.Pendiente);
        return pedidos
            .Where(p => p.EstadoPago == EstadoPagoEnum.Pagado)
            .Select(p => new PedidoPendientePagoDto
            {
                Id = p.Id,
                ClienteNombre = p.ClienteOnline?.Nombre ?? string.Empty,
                Total = p.Total,
                MontoConDescuento = p.MontoConDescuento,
                TipoPago = p.TipoPago?.ToString(),
                ReferenciaTransaccion = p.ReferenciaTransaccion,
                FechaPago = p.FechaPago ?? DateTime.MinValue,
                FechaPedido = p.FechaPedido
            })
            .ToList();
    }

    public async Task<PedidoDto> ConfirmarPagoPedidoAsync(int pedidoId)
    {
        var pedido = await pedidoRepository.GetByIdWithDetallesAsync(pedidoId);
        if (pedido == null)
            throw new NotFoundException("Pedido", pedidoId);

        if (pedido.EstadoPago == EstadoPagoEnum.Pagado && pedido.StockDescontado)
            throw new BadRequestException("El pedido ya fue confirmado y el stock descontado");

        if (pedido.Estado != EstadoPedidoEnum.Pendiente)
            throw new BadRequestException("El pedido no está pendiente de confirmación");

        // Si el cliente ya marcó como Pagado o viene directo de Efectivo
        if (pedido.EstadoPago == EstadoPagoEnum.Pendiente)
            pedido.EstadoPago = EstadoPagoEnum.Pagado;

        pedido.Estado = EstadoPedidoEnum.Confirmado;
        pedido.StockDescontado = true;

        foreach (var detalle in pedido.Detalles)
        {
            detalle.Producto?.DescontarStock(detalle.Cantidad);
        }

        var updated = await pedidoRepository.UpdateAsync(pedido);
        return updated.Adapt<PedidoDto>();
    }
}
