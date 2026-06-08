
namespace Softpan.Application.DTOs;

// DTO para respuesta - Información básica del producto
public class ProductoDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public decimal PrecioBase { get; set; }
    public string? Categoria { get; set; }
    public string? ImagenUrl { get; set; }
    public int Stock { get; set; }
    public int StockMinimo { get; set; }
    public bool Activo { get; set; }
    public DateTime FechaCreacion { get; set; }
    public DateTime? FechaModificacion { get; set; }
    public bool StockInmediato { get; set; }
    public bool EnOferta { get; set; }
    public decimal? PrecioOferta { get; set; }
    public List<ProductoImagenDto> Imagenes { get; set; } = new();
}

// DTO para respuesta detallada - Incluye precios personalizados
public class ProductoDetalleDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public decimal PrecioBase { get; set; }
    public string? Categoria { get; set; }
    public string? ImagenUrl { get; set; }
    public int Stock { get; set; }
    public int StockMinimo { get; set; }
    public bool Activo { get; set; }
    public DateTime FechaCreacion { get; set; }
    public DateTime? FechaModificacion { get; set; }
    public List<PrecioPersonalizadoDto> PreciosPersonalizados { get; set; } = new();
    public List<ProductoImagenDto> Imagenes { get; set; } = new();
}

// DTO para precios personalizados dentro de ProductoDetalleDto
public class PrecioPersonalizadoDto
{
    public int Id { get; set; }
    public int ClienteId { get; set; }
    public string ClienteNombre { get; set; } = string.Empty;
    public decimal Precio { get; set; }
}

// DTO para crear producto
public class CreateProductoDto
{
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public string? Categoria { get; set; }
    public string? ImagenUrl { get; set; }
    public decimal PrecioBase { get; set; }
    public int Stock { get; set; } = 0;
    public int StockMinimo { get; set; } = 5;
    public bool StockInmediato { get; set; }
    public bool EnOferta { get; set; }
    public decimal? PrecioOferta { get; set; }
}

// DTO para actualizar producto
public class UpdateProductoDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public string? Categoria { get; set; }
    public string? ImagenUrl { get; set; }
    public decimal PrecioBase { get; set; }
    public int Stock { get; set; }
    public int StockMinimo { get; set; }
    public bool Activo { get; set; } = true;
    public bool StockInmediato { get; set; }
    public bool EnOferta { get; set; }
    public decimal? PrecioOferta { get; set; }
}

// DTO para actualizar solo el stock
public class UpdateStockDto
{
    public int Stock { get; set; }
}