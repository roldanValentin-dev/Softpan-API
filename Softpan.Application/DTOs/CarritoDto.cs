

namespace Softpan.Application.DTOs;

public class CarritoDto
{
    public int PedidoId { get; set; }
    public decimal Total { get; set; }
    public int TotalItems { get; set; }
    public List<CarritoItemDto> Items { get; set; } = [];
}

public class CarritoItemDto
{
    public int ProductoId { get; set; }
    public string ProductoNombre { get; set; } = string.Empty;
    public string? ProductoImagen { get; set; }
    public string? ProductoCategoria { get; set; }
    public decimal PrecioUnitario { get; set; }
    public int Cantidad { get; set; }
    public decimal Subtotal { get; set; }

}
public class AgregarItemCarritoDto
{
    public int ProductoId { get; set; }
    public int Cantidad { get; set; }
}
public class ActualizarItemCarritoDto
{
    public int Cantidad { get; set; }
}
public class ProcesarCheckoutDto
{
    public DateTime FechaEntrega { get; set; }
    public string? Observaciones { get; set; }
}
