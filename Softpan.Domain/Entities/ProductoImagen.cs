namespace Softpan.Domain.Entities;

public class ProductoImagen
{
    public int Id { get; set; }
    public int ProductoId { get; set; }
    public Producto Producto { get; set; } = null!;
    
    public string Url { get; set; } = string.Empty;
    public int Orden { get; set; } = 0;
    public bool EsPrincipal { get; set; } = false;
    
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
}
