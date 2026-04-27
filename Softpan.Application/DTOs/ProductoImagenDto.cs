namespace Softpan.Application.DTOs;

public class ProductoImagenDto
{
    public int Id { get; set; }
    public int ProductoId { get; set; }
    public string Url { get; set; } = string.Empty;
    public int Orden { get; set; }
    public bool EsPrincipal { get; set; }
    public DateTime FechaCreacion { get; set; }
}

public class CreateProductoImagenDto
{
    public string Url { get; set; } = string.Empty;
    public int Orden { get; set; } = 0;
    public bool EsPrincipal { get; set; } = false;
}

public class UpdateProductoImagenDto
{
    public int Orden { get; set; }
    public bool EsPrincipal { get; set; }
}
