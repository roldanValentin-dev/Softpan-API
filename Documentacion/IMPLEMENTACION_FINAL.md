# Plan de Implementación Final — Softpan API

## Funcionalidades para dejar la API lista para producción

---

## 1. 🔓 Contraseña menos exigente

**Objetivo**: Simplificar los requisitos de contraseña para que sea más fácil registrarse.

### Cambios

**Archivo**: `Softpan.API/Program.cs` (líneas 88-96)

**Antes:**
```csharp
options.Password.RequireDigit = true;
options.Password.RequireLowercase = true;
options.Password.RequireUppercase = true;
options.Password.RequireNonAlphanumeric = false;
options.Password.RequiredLength = 6;
```

**Después:**
```csharp
options.Password.RequireDigit = false;
options.Password.RequireLowercase = false;
options.Password.RequireUppercase = false;
options.Password.RequireNonAlphanumeric = false;
options.Password.RequiredLength = 4;
```

### Frontend
- Actualizar validaciones del formulario de registro

---

## 2. 📦 Stock inmediato (checkbox en admin)

**Objetivo**: El admin marca productos que están listos para retirar el mismo día. Se muestran en un banner en el catálogo.

### Cambios

### 2.1 Domain

**Archivo**: `Softpan.Domain/Entities/Producto.cs`

Agregar campo:
```csharp
public bool StockInmediato { get; set; } = false;
```

### 2.2 Application — DTOs

**Archivo**: `Softpan.Application/DTOs/ProductoDto.cs`

Agregar:
```csharp
public bool StockInmediato { get; set; }
```

**Archivo**: `Softpan.Application/DTOs/CreateProductoDto.cs`

Agregar:
```csharp
public bool StockInmediato { get; set; }
```

**Archivo**: `Softpan.Application/DTOs/UpdateProductoDto.cs`

Agregar:
```csharp
public bool StockInmediato { get; set; }
```

### 2.3 Application — Services

**Archivo**: `Softpan.Application/Services/ProductoService.cs`

Agregar método:
```csharp
public async Task<IEnumerable<ProductoDto>> GetProductosInmediatoAsync()
{
    // Retorna productos activos con StockInmediato = true
    var cacheKey = "productos:inmediato";
    var cacheProductos = await cacheService.GetAsync<IEnumerable<ProductoDto>>(cacheKey);
    if (cacheProductos != null) return cacheProductos;

    var productos = await productoRepository.GetProductosInmediatoAsync();
    var dto = productos.Select(MapToDto).ToList();
    await cacheService.SetAsync(cacheKey, dto, TimeSpan.FromMinutes(5));
    return dto;
}
```

### 2.4 Domain — Repository Interface

**Archivo**: `Softpan.Domain.Interfaces/IProductoRepository.cs`

Agregar:
```csharp
Task<IEnumerable<Producto>> GetProductosInmediatoAsync();
```

### 2.5 Infrastructure — Repository

**Archivo**: `Softpan.Infrastructure/Repositories/ProductoRepository.cs`

Agregar:
```csharp
public async Task<IEnumerable<Producto>> GetProductosInmediatoAsync()
{
    return await context.Productos
        .AsNoTracking()
        .Include(p => p.Imagenes)
        .Where(p => p.Activo && p.StockInmediato)
        .ToListAsync();
}
```

### 2.6 API — Controller

**Archivo**: `Softpan.API/Controllers/CatalogoController.cs`

Agregar endpoint:
```csharp
[HttpGet("productos/inmediato")]
public async Task<IActionResult> GetProductosInmediato()
{
    var productos = await productoService.GetProductosInmediatoAsync();
    return Ok(productos);
}
```

### 2.7 Migración

```bash
dotnet ef migrations add AddStockInmediato --project Softpan.Infrastructure --startup-project Softpan.API
```

### Frontend
- Admin: checkbox "Stock inmediato" en formulario de producto
- Catálogo: banner/pestaña "Retiro inmediato" que llama a `GET /api/catalogo/productos/inmediato`
- Card de producto: badge "Retiro hoy"

---

## 3. 🏷️ Productos en oferta

**Objetivo**: El admin marca un producto en oferta con un precio rebajado. Se muestra en el catálogo.

### Cambios

### 3.1 Domain

**Archivo**: `Softpan.Domain/Entities/Producto.cs`

Agregar campos:
```csharp
public bool EnOferta { get; set; } = false;
public decimal? PrecioOferta { get; set; }
```

### 3.2 Application — DTOs

**Archivo**: `Softpan.Application/DTOs/ProductoDto.cs`

Agregar:
```csharp
public bool EnOferta { get; set; }
public decimal? PrecioOferta { get; set; }
```

**Archivo**: `Softpan.Application/DTOs/CreateProductoDto.cs`

Agregar:
```csharp
public bool EnOferta { get; set; }
public decimal? PrecioOferta { get; set; }
```

**Archivo**: `Softpan.Application/DTOs/UpdateProductoDto.cs`

Agregar:
```csharp
public bool EnOferta { get; set; }
public decimal? PrecioOferta { get; set; }
```

### 3.3 Application — Services

**Archivo**: `Softpan.Application/Services/ProductoService.cs`

Agregar método:
```csharp
public async Task<IEnumerable<ProductoDto>> GetProductosEnOfertaAsync()
{
    var cacheKey = "productos:oferta";
    var cacheProductos = await cacheService.GetAsync<IEnumerable<ProductoDto>>(cacheKey);
    if (cacheProductos != null) return cacheProductos;

    var productos = await productoRepository.GetProductosEnOfertaAsync();
    var dto = productos.Select(MapToDto).ToList();
    await cacheService.SetAsync(cacheKey, dto, TimeSpan.FromMinutes(5));
    return dto;
}
```

Actualizar `MapToDto` para incluir `EnOferta` y `PrecioOferta`.

### 3.4 Domain — Repository Interface

**Archivo**: `Softpan.Domain.Interfaces/IProductoRepository.cs`

Agregar:
```csharp
Task<IEnumerable<Producto>> GetProductosEnOfertaAsync();
```

### 3.5 Infrastructure — Repository

**Archivo**: `Softpan.Infrastructure/Repositories/ProductoRepository.cs`

Agregar:
```csharp
public async Task<IEnumerable<Producto>> GetProductosEnOfertaAsync()
{
    return await context.Productos
        .AsNoTracking()
        .Include(p => p.Imagenes)
        .Where(p => p.Activo && p.EnOferta && p.PrecioOferta != null)
        .ToListAsync();
}
```

### 3.6 API — Controller

**Archivo**: `Softpan.API/Controllers/CatalogoController.cs`

Agregar endpoint:
```csharp
[HttpGet("productos/oferta")]
public async Task<IActionResult> GetProductosEnOferta()
{
    var productos = await productoService.GetProductosEnOfertaAsync();
    return Ok(productos);
}
```

### 3.7 Migración

Junto con StockInmediato (misma migración):
```bash
dotnet ef migrations add AddCamposProducto --project Softpan.Infrastructure --startup-project Softpan.API
```

### Frontend
- Admin: checkbox "En oferta" + campo "Precio de oferta" en formulario de producto
- Catálogo: badge "OFERTA" + precio tachado + precio de oferta
- Endpoint separado: `GET /api/catalogo/productos/oferta`

---

## 4. 🛒 Carrito mixto (inmediato + producción)

**Objetivo**: Si el carrito tiene productos inmediatos y no inmediatos, el backend separa en 2 pedidos al hacer checkout. Para MercadoPago se genera una sola preferencia por el total.

### Cambios

### 4.1 Domain — Entidad Carrito/Pedido

No se necesitan campos nuevos. La lógica se maneja desde el service.

### 4.2 Application — Services

**Archivo**: `Softpan.Application/Services/PedidoService.cs`

**Método `CreatePedidoAsync`**: Detectar si hay mix de stock inmediato. Si lo hay, crear 2 pedidos:

```csharp
public async Task<PedidoDto> CreatePedidoAsync(CreatePedidoDto dto, string usuarioIdentity)
{
    var cliente = await clienteOnlineRepository.GetByUsuarioIdentityIdAsync(usuarioIdentity);
    if (cliente == null) throw new NotFoundException("Cliente no encontrado");

    var detallesInmediatos = dto.Detalles.Where(d => EsProductoInmediato(d.ProductoId)).ToList();
    var detallesProduccion = dto.Detalles.Where(d => !EsProductoInmediato(d.ProductoId)).ToList();

    // Si hay mix, crear 2 pedidos
    if (detallesInmediatos.Count > 0 && detallesProduccion.Count > 0)
    {
        var pedidoInmediato = await CrearUnPedido(cliente, detallesInmediatos, dto, esInmediato: true);
        var pedidoProduccion = await CrearUnPedido(cliente, detallesProduccion, dto, esInmediato: false);
        // Vincularlos (campo PedidoPadreId opcional)
        return MapToDto(pedidoInmediato);
    }

    // Si no hay mix, crear un solo pedido
    return await CrearUnPedido(cliente, dto.Detalles, dto, esInmediato: null);
}
```

**Método `ProcesarCheckoutDesdeCarritoAsync`**: Misma lógica de separación.

**Método para MercadoPago**: Generar una preferencia que sume el total de ambos pedidos:

```csharp
// En MercadoPagoService, aceptar múltiples pedidos
public async Task<MercadoPagoPreferenceResponseDto> CrearPreferenciaPagoParaPedidosAsync(
    List<int> pedidosIds, string? emailPagador)
{
    // external_reference = "101,102"
    // Sumar items de todos los pedidos
    // Total global = suma + costo de envío (una vez)
}
```

**Webhook (`MercadoPagoService.ProcesarWebhookMercadoPagoAsync`)**:

Actualizar el parseo de `external_reference` para soportar múltiples IDs:
```csharp
// Antes:
if (!int.TryParse(externalRef, out var pedidoId))

// Después:
var pedidosIds = externalRef.Split(',').Select(int.Parse).ToList();
foreach (var pedidoId in pedidosIds) { ... }
```

### Frontend
- Carrito: separar visualmente "Retiro inmediato" y "A producir"
- Checkout: mostrar fecha de entrega diferenciada
- Mis pedidos: mostrar ambos pedidos vinculados (mismo número de referencia)
- Ocultar MP si el carrito es mixto (opcional para simplificar)
- Mostrar "Envío" o "Retiro" como opción de entrega

---

## 5. 💰 Costo de envío fijo

**Objetivo**: Configurar desde el admin una tarifa fija de envío y un monto mínimo para envío gratis. Al crear un pedido, si el cliente elige envío, se suma el costo al total.

### Cambios

### 5.1 Domain

**Archivo**: `Softpan.Domain/Entities/Pedido.cs`

Agregar campos:
```csharp
public decimal? CostoEnvio { get; set; }
public string? DireccionEntrega { get; set; }
public bool EsRetiroLocal { get; set; } = true;
```

### 5.2 Application — DTOs

**Archivo**: `Softpan.Application/DTOs/PedidoDto.cs`

Agregar:
```csharp
public decimal? CostoEnvio { get; set; }
public string? DireccionEntrega { get; set; }
public bool EsRetiroLocal { get; set; }
```

**Archivo**: `Softpan.Application/DTOs/CreatePedidoDto.cs`

Agregar:
```csharp
public bool EsRetiroLocal { get; set; } = true;
public string? DireccionEntrega { get; set; }
```

**Archivo**: `Softpan.Application/DTOs/AdminConfigDto.cs`

Agregar:
```csharp
public class CostoEnvioConfigDto
{
    public decimal CostoEnvio { get; set; }
    public decimal? MinimoGratis { get; set; }
}
```

### 5.3 Application — Interfaces

**Archivo**: `Softpan.Application/Interfaces/IAdminPagoService.cs`

Agregar:
```csharp
Task<CostoEnvioConfigDto> GetCostoEnvioConfigAsync();
Task<CostoEnvioConfigDto> UpdateCostoEnvioConfigAsync(CostoEnvioConfigDto dto);
```

### 5.4 Application — Services

**Archivo**: `Softpan.Application/Services/AdminPagoService.cs`

Agregar:
```csharp
private const string CLAVE_COSTO_ENVIO = "CostoEnvio";
private const string CLAVE_MINIMO_GRATIS = "CostoEnvioMinimoGratis";

public async Task<CostoEnvioConfigDto> GetCostoEnvioConfigAsync()
{
    var costo = (await configuracionRepository.GetByClaveAsync(CLAVE_COSTO_ENVIO))?.Valor;
    var minimo = (await configuracionRepository.GetByClaveAsync(CLAVE_MINIMO_GRATIS))?.Valor;

    return new CostoEnvioConfigDto
    {
        CostoEnvio = decimal.TryParse(costo, out var c) ? c : 0,
        MinimoGratis = decimal.TryParse(minimo, out var m) ? m : null
    };
}

public async Task<CostoEnvioConfigDto> UpdateCostoEnvioConfigAsync(CostoEnvioConfigDto dto)
{
    await configuracionRepository.SetAsync(CLAVE_COSTO_ENVIO, dto.CostoEnvio.ToString());
    await configuracionRepository.SetAsync(CLAVE_MINIMO_GRATIS, dto.MinimoGratis?.ToString() ?? "");
    return dto;
}
```

**Archivo**: `Softpan.Application/Services/PedidoService.cs`

En `CreatePedidoAsync`: calcular costo de envío:

```csharp
private async Task<decimal> CalcularCostoEnvioAsync(decimal totalProductos, bool esRetiroLocal)
{
    if (esRetiroLocal) return 0;

    var config = await adminPagoService.GetCostoEnvioConfigAsync();
    if (config.MinimoGratis.HasValue && totalProductos >= config.MinimoGratis.Value)
        return 0;

    return config.CostoEnvio;
}
```

Actualizar `CalcularTotal` en `Pedido.cs`:
```csharp
public void CalcularTotal()
{
    Total = Detalles.Sum(d => d.Subtotal) + (CostoEnvio ?? 0);
}
```

### 5.5 API — Controller

**Archivo**: `Softpan.API/Controllers/AdminPagoController.cs`

Agregar:
```csharp
[HttpGet("envio/config")]
public async Task<IActionResult> GetCostoEnvioConfig()
{
    var config = await adminPagoService.GetCostoEnvioConfigAsync();
    return Ok(config);
}

[HttpPut("envio/config")]
public async Task<IActionResult> UpdateCostoEnvioConfig([FromBody] CostoEnvioConfigDto dto)
{
    var config = await adminPagoService.UpdateCostoEnvioConfigAsync(dto);
    return Ok(config);
}
```

### 5.6 Migración

```bash
dotnet ef migrations add AddCostoEnvioPedido --project Softpan.Infrastructure --startup-project Softpan.API
```

### Frontend
- Panel admin: sección "Configurar envío" (tarifa fija + monto mínimo gratis)
- Checkout: radio buttons "Retiro en el local" / "Envío a domicilio"
- Si elige envío: mostrar campo de dirección + costo estimado
- Carrito: mostrar línea "Envío: $XXX" o "Envío gratis"
- Mis pedidos: mostrar dirección y costo de envío

---

## 6. 📧 Email (SendGrid)

**Objetivo**: Enviar emails de notificaciones al cliente (pedido creado, cambio de estado) y email de recuperación de contraseña.

### 6.1 Dependencia NuGet

```bash
dotnet add Softpan.Infrastructure package SendGrid
```

### 6.2 Interfaz

**Archivo**: `Softpan.Application/Interfaces/IEmailService.cs` (NUEVO)

```csharp
namespace Softpan.Application.Interfaces;

public interface IEmailService
{
    Task SendEmailAsync(string to, string subject, string htmlBody);
}
```

### 6.3 Implementación

**Archivo**: `Softpan.Infrastructure/Services/EmailService.cs` (NUEVO)

```csharp
using SendGrid;
using SendGrid.Helpers.Mail;
using Softpan.Application.Interfaces;

namespace Softpan.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly string _apiKey;
    private readonly string _fromEmail;
    private readonly string _fromName;

    public EmailService(IConfiguration configuration)
    {
        _apiKey = configuration["SendGrid:ApiKey"]
            ?? throw new InvalidOperationException("SendGrid:ApiKey no configurado");
        _fromEmail = configuration["SendGrid:FromEmail"] ?? "noreply@softpan.com";
        _fromName = configuration["SendGrid:FromName"] ?? "Softpan";
    }

    public async Task SendEmailAsync(string to, string subject, string htmlBody)
    {
        var client = new SendGridClient(_apiKey);
        var from = new EmailAddress(_fromEmail, _fromName);
        var toAddress = new EmailAddress(to);
        var msg = MailHelper.CreateSingleEmail(from, toAddress, subject, plainTextContent: null, htmlBody);
        await client.SendEmailAsync(msg);
    }
}
```

### 6.4 Templates de emails

**Archivo**: `Softpan.Infrastructure/Services/EmailTemplates.cs` (NUEVO)

```csharp
namespace Softpan.Infrastructure.Services;

public static class EmailTemplates
{
    public static string PedidoCreado(string clienteNombre, int pedidoId, decimal total, string estado)
    {
        return $"""
        <h2>¡Pedido confirmado!</h2>
        <p>Hola {clienteNombre},</p>
        <p>Tu pedido <strong>#{pedidoId}</strong> fue creado exitosamente.</p>
        <p><strong>Total:</strong> ${total}</p>
        <p><strong>Estado:</strong> {estado}</p>
        """;
    }

    public static string PedidoEstadoActualizado(string clienteNombre, int pedidoId, string estadoNuevo)
    {
        return $"""
        <h2>Estado de tu pedido actualizado</h2>
        <p>Hola {clienteNombre},</p>
        <p>Tu pedido <strong>#{pedidoId}</strong> ahora está: <strong>{estadoNuevo}</strong></p>
        """;
    }

    public static string ResetPassword(string clienteNombre, string resetLink)
    {
        return $"""
        <h2>Recuperación de contraseña</h2>
        <p>Hola {clienteNombre},</p>
        <p>Hacé clic en el siguiente enlace para restablecer tu contraseña:</p>
        <p><a href="{resetLink}">Restablecer contraseña</a></p>
        <p>Si no solicitaste esto, ignorá este mensaje.</p>
        """;
    }
}
```

### 6.5 DI Registration

**Archivo**: `Softpan.Infrastructure/DependencyInjections.cs`

Agregar:
```csharp
services.AddScoped<IEmailService, EmailService>();
```

### 6.6 Notificaciones en PedidoService

**Archivo**: `Softpan.Application/Services/PedidoService.cs`

Inyectar `IEmailService` en el constructor. Enviar email en:

- `CreatePedidoAsync`: Email de confirmación al cliente
- `UpdateEstadoPedidoAsync`: Email con nuevo estado
- `CancelarPedidoAsync`: Email de cancelación

### 6.7 Variables de entorno

**Archivo**: `.env.example`

Agregar:
```env
# SendGrid
SENDGRID_API_KEY=SG.xxxxxxxxxx
SENDGRID_FROM_EMAIL=noreply@tudominio.com
SENDGRID_FROM_NAME=TuTienda
```

### Frontend
- No requiere cambios, los emails se envían automáticamente desde el backend

---

## 7. 🔐 Recuperación de contraseña

**Objetivo**: El cliente puede solicitar un email para restablecer su contraseña olvidada.

### 7.1 DTOs

**Archivo**: `Softpan.Application/DTOs/AuthDto.cs`

Agregar:
```csharp
public class ForgotPasswordDto
{
    public string Email { get; set; } = string.Empty;
}

public class ResetPasswordDto
{
    public string Email { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}
```

### 7.2 Application — Interfaces

**Archivo**: `Softpan.Application/Interfaces/IAuthService.cs`

Agregar:
```csharp
Task ForgotPasswordAsync(ForgotPasswordDto dto);
Task ResetPasswordAsync(ResetPasswordDto dto);
```

### 7.3 Application — Services

**Archivo**: `Softpan.Application/Services/AuthService.cs`

Agregar:
```csharp
private readonly IEmailService _emailService;
private readonly IConfiguration _configuration;

public async Task ForgotPasswordAsync(ForgotPasswordDto dto)
{
    var user = await userManager.FindByEmailAsync(dto.Email);
    if (user == null) return; // No revelar si el email existe o no

    var token = await userManager.GeneratePasswordResetTokenAsync(user);
    var baseUrl = configuration["MercadoPago:BaseUrl"] ?? "http://localhost:5173";
    var resetLink = $"{baseUrl}/reset-password?email={Uri.EscapeDataString(dto.Email)}&token={Uri.EscapeDataString(token)}";

    await _emailService.SendEmailAsync(
        dto.Email,
        "Recuperación de contraseña",
        EmailTemplates.ResetPassword($"{user.FirstName} {user.LastName}", resetLink)
    );
}

public async Task ResetPasswordAsync(ResetPasswordDto dto)
{
    var user = await userManager.FindByEmailAsync(dto.Email);
    if (user == null) throw new BadRequestException("Solicitud inválida");

    var result = await userManager.ResetPasswordAsync(user, dto.Token, dto.NewPassword);
    if (!result.Succeeded)
        throw new BadRequestException(string.Join(", ", result.Errors.Select(e => e.Description)));
}
```

### 7.4 API — Controller

**Archivo**: `Softpan.API/Controllers/AuthController.cs`

Agregar:
```csharp
[HttpPost("forgot-password")]
public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
{
    await authService.ForgotPasswordAsync(dto);
    return Ok(new { message = "Si el email existe, recibirás un enlace de recuperación" });
}

[HttpPost("reset-password")]
public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
{
    await authService.ResetPasswordAsync(dto);
    return Ok(new { message = "Contraseña actualizada exitosamente" });
}
```

### Frontend
- Página `/forgot-password` con formulario de email
- Página `/reset-password` con token + nueva contraseña
- Mensaje de éxito/error

---

## 8. ☁️ Imágenes en la nube (Cloudinary)

**Objetivo**: Reemplazar el almacenamiento local de imágenes por Cloudinary. El frontend no se entera del cambio.

### 8.1 Dependencia NuGet

```bash
dotnet add Softpan.Infrastructure package CloudinaryDotNet
```

### 8.2 Implementación

**Archivo**: `Softpan.Infrastructure/Services/CloudinaryFileStorageService.cs` (NUEVO)

```csharp
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Softpan.Application.Interfaces;

namespace Softpan.Infrastructure.Services;

public class CloudinaryFileStorageService : IFileStorageService
{
    private readonly Cloudinary _cloudinary;
    private readonly string[] _allowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
    private readonly long _maxFileSize = 5 * 1024 * 1024;

    public CloudinaryFileStorageService(IConfiguration configuration)
    {
        var cloudName = configuration["Cloudinary:CloudName"]
            ?? throw new InvalidOperationException("Cloudinary:CloudName no configurado");
        var apiKey = configuration["Cloudinary:ApiKey"]
            ?? throw new InvalidOperationException("Cloudinary:ApiKey no configurado");
        var apiSecret = configuration["Cloudinary:ApiSecret"]
            ?? throw new InvalidOperationException("Cloudinary:ApiSecret no configurado");

        var account = new Account(cloudName, apiKey, apiSecret);
        _cloudinary = new Cloudinary(account);
    }

    public async Task<string> SaveFileAsync(Stream fileStream, string fileName, string folder)
    {
        var (isValid, errorMessage) = ValidateImageFile(fileName, fileStream.Length);
        if (!isValid) throw new InvalidOperationException(errorMessage);

        var uploadParams = new ImageUploadParams
        {
            File = new FileDescription(fileName, fileStream),
            Folder = $"softpan/{folder}",
            UseFilename = true,
            UniqueFilename = true
        };

        var result = await _cloudinary.UploadAsync(uploadParams);

        if (result.Error != null)
            throw new InvalidOperationException($"Error al subir imagen: {result.Error.Message}");

        return result.SecureUrl.ToString();
    }

    public Task<bool> DeleteFileAsync(string fileUrl)
    {
        // Extraer public ID de la URL
        // Ej: https://res.cloudinary.com/.../softpan/productos/abc123.jpg
        var uri = new Uri(fileUrl);
        var segments = uri.Segments;
        var publicId = string.Concat(segments
            .SkipWhile(s => !s.Contains("softpan"))
            .Select(s => s.TrimEnd('/')));
        publicId = Path.GetFileNameWithoutExtension(publicId);

        var deleteParams = new DeletionParams(publicId);
        var result = _cloudinary.Destroy(deleteParams);

        return Task.FromResult(result.Result == "ok");
    }

    public (bool isValid, string errorMessage) ValidateImageFile(string fileName, long fileSize)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        if (!_allowedExtensions.Contains(extension))
            return (false, $"Extensión no permitida. Solo: {string.Join(", ", _allowedExtensions)}");
        if (fileSize > _maxFileSize)
            return (false, $"Máximo permitido: {_maxFileSize / 1024 / 1024} MB");
        if (fileSize == 0)
            return (false, "El archivo está vacío");
        return (true, string.Empty);
    }
}
```

### 8.3 DI Registration

**Archivo**: `Softpan.Infrastructure/DependencyInjections.cs`

Reemplazar:
```csharp
// Antes:
services.AddScoped<IFileStorageService, LocalFileStorageService>();

// Después:
services.AddScoped<IFileStorageService, CloudinaryFileStorageService>();
```

Mantener `LocalFileStorageService` registrado como fallback opcional para desarrollo local.

### 8.4 Variables de entorno

**Archivo**: `.env.example`

Agregar:
```env
# Cloudinary (imágenes)
CLOUDINARY_CLOUD_NAME=tu-cloud-name
CLOUDINARY_API_KEY=123456789
CLOUDINARY_API_SECRET=abc123def456
```

### Frontend
- No requiere cambios. Las URLs que devuelve la API ahora apuntan a Cloudinary en vez de `/images/...`

---

## 📋 Orden de implementación sugerido

| Orden | Feature | Días |
|-------|---------|------|
| 1 | Contraseña simple | 0.1 |
| 2 | Stock inmediato + Ofertas (juntos, misma migración) | 1 |
| 3 | Costo de envío fijo | 1 |
| 4 | SendGrid + EmailService | 1 |
| 5 | Password reset | 0.5 |
| 6 | Notificaciones email en PedidoService | 0.5 |
| 7 | Carrito mixto | 1.5 |
| 8 | Cloudinary | 1 |
| | **Total** | **~6.5 días** |

---

## 📁 Resumen de archivos a crear (6)

| # | Archivo |
|---|---------|
| 1 | `Application/Interfaces/IEmailService.cs` |
| 2 | `Infrastructure/Services/EmailService.cs` |
| 3 | `Infrastructure/Services/EmailTemplates.cs` |
| 4 | `Infrastructure/Services/CloudinaryFileStorageService.cs` |
| 5 | `Application/DTOs/AuthDto.cs` (agregar ForgotPasswordDto, ResetPasswordDto) |
| 6 | `Application/DTOs/CostoEnvioConfigDto.cs` (o dentro de AdminConfigDto) |

## 📁 Resumen de archivos a modificar (~16)

| # | Archivo |
|---|---------|
| 1 | `API/Program.cs` |
| 2 | `Domain/Entities/Producto.cs` |
| 3 | `Domain/Entities/Pedido.cs` |
| 4 | `Domain.Interfaces/IProductoRepository.cs` |
| 5 | `Application/DTOs/ProductoDto.cs` |
| 6 | `Application/DTOs/CreateProductoDto.cs` |
| 7 | `Application/DTOs/UpdateProductoDto.cs` |
| 8 | `Application/DTOs/PedidoDto.cs` |
| 9 | `Application/DTOs/CreatePedidoDto.cs` |
| 10 | `Application/DTOs/AdminConfigDto.cs` |
| 11 | `Application/Services/ProductoService.cs` |
| 12 | `Application/Services/PedidoService.cs` |
| 13 | `Application/Services/AdminPagoService.cs` |
| 14 | `Application/Services/AuthService.cs` |
| 15 | `Infrastructure/Repositories/ProductoRepository.cs` |
| 16 | `Infrastructure/DependencyInjections.cs` |
| 17 | `API/Controllers/CatalogoController.cs` |
| 18 | `API/Controllers/AuthController.cs` |
| 19 | `API/Controllers/AdminPagoController.cs` |
| 20 | `.env.example` |

---

## Variables de entorno finales

```env
# ASP.NET Core
ASPNETCORE_ENVIRONMENT=Development

# Base de datos
DB_PASSWORD=CambiarEstaContrasena123!

# JWT
JWT_KEY=ClaveSuperSeguraDe32CaracteresMinimo
JWT_ISSUER=SoftpanAPI
JWT_AUDIENCE=SoftpanFrontend

# Mercado Pago
MP_ACCESS_TOKEN=APP_USR-xxxxxxxxxx
MP_CLIENT_SECRET=xxxxxxxxxx
MP_BASE_URL=http://localhost:5173
MP_NOTIFICATION_URL=https://tu-dominio.ngrok-free.dev/api/mercadopago/webhook

# SendGrid (emails)
SENDGRID_API_KEY=SG.xxxxxxxxxx
SENDGRID_FROM_EMAIL=noreply@tudominio.com
SENDGRID_FROM_NAME=TuTienda

# Cloudinary (imágenes)
CLOUDINARY_CLOUD_NAME=tu-cloud-name
CLOUDINARY_API_KEY=123456789
CLOUDINARY_API_SECRET=abc123def456

# Puerto API
API_PORT=7097
```

---

## Frontend (pendiente, a cargo tuyo)

### Nuevas páginas/rutas
| Ruta | Componente | Descripción |
|------|-----------|-------------|
| `/forgot-password` | `ForgotPassword.jsx` | Ingresar email para reset |
| `/reset-password` | `ResetPassword.jsx` | Ingresar token + nueva contraseña |

### Cambios en componentes existentes
| Componente | Cambio |
|-----------|--------|
| `AdminProductos.jsx` (form) | Agregar checkbox "Stock inmediato", checkbox "En oferta", campo "Precio de oferta" |
| `ProductsList.jsx` | Agregar pestaña/banner "Retiro inmediato" |
| `ProductDetail.jsx` | Mostrar badge "Oferta" con precio tachado + oferta |
| `Cart.jsx` | Mostrar "Envío: $XXX" y separar items inmediatos/producción |
| `Checkout.jsx` | Selector "Retiro local" / "Envío a domicilio", mostrar costo de envío |
| `MisPedidos.jsx` | Mostrar pedidos vinculados, dirección de entrega, costo de envío |
| `AdminConfigPago.jsx` | Agregar sección "Configurar envío" (tarifa + mínimo gratis) |
