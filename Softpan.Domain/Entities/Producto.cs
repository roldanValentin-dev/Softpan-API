
namespace Softpan.Domain.Entities;

public class Producto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public decimal PrecioBase { get; set; }
    public string? Categoria { get; set; }  
    public string? ImagenUrl { get; set; }
    public int Stock { get; set; } = 0;
    public int StockMinimo { get; set; } = 5;
    public bool Activo { get; set; } = true;
    public bool StockInmediato { get; set; } = false;
    public bool EnOferta { get; set; } = false;
    public decimal? PrecioOferta { get; set; }
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    public DateTime? FechaModificacion { get; set; }


    public ICollection<DetalleVenta> DetallesVenta { get; set; } = new List<DetalleVenta>();
    public ICollection<PrecioCliente> PreciosPersonalizados { get; set; } = new List<PrecioCliente>();
    public ICollection<ProductoImagen> Imagenes { get; set; } = new List<ProductoImagen>();

    // Métodos de negocio para stock
    public bool TieneStock(int cantidad) => Stock >= cantidad;
    public void DescontarStock(int cantidad)
    {
        if (cantidad > Stock)
            throw new InvalidOperationException($"Stock insuficiente. Disponible: {Stock}, Solicitado: {cantidad}");
        Stock -= cantidad;
    }
    public void RestaurarStock(int cantidad) => Stock += cantidad;
    public bool StockBajo() => Stock <= StockMinimo;
}
