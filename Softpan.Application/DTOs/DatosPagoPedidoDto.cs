namespace Softpan.Application.DTOs;

public class DatosPagoPedidoDto
{
    public int PedidoId { get; set; }
    public decimal Total { get; set; }
    public decimal? MontoConDescuento { get; set; }
    public string? TipoPago { get; set; }
    public string EstadoPago { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty;
    public string? DireccionRetiro { get; set; }
    public string? HorarioRetiro { get; set; }
    public string? TelefonoContacto { get; set; }
    public DatosBancariosDto? DatosBancarios { get; set; }
}
