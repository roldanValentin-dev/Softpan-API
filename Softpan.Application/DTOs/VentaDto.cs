

using Softpan.Domain.Enums;

namespace Softpan.Application.DTOs;
public class VentaDto
{
    public int Id { get; set; }
    public int ClienteId { get; set; }
    public string ClienteNombre { get; set; } = string.Empty;  
    public DateTime FechaCreacion { get; set; }                
    public DateTime? FechaModificacion { get; set; }          
    public decimal MontoTotal { get; set; }                    
    public decimal MontoPagado { get; set; }                   
    public decimal SaldoPendiente { get; set; }                
    public EstadoVentaEnum Estado { get; set; }                
    public string EstadoNombre { get; set; } = string.Empty;  
    public List<DetalleVentaDto> Detalles { get; set; } = new(); 

}

public class DetalleVentaDto
{
    public int Id { get; set; }
    public int ProductoId { get; set; }
    public string ProductoNombre { get; set; } = string.Empty;
    public int Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
    public decimal Subtotal { get; set; }
}
public class CreateVentaDto
{
    public int ClienteId { get; set; }
    public List<CreateDetalleVentaDto> Detalles { get; set; } = new();
}

public class CreateDetalleVentaDto
{
    public int ProductoId { get; set; }
    public int Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
}