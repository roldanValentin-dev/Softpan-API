
namespace Softpan.Domain.Entities;

public class PagoVenta
{
    public int Id { get; set; }
    public int PagoId { get; set; }
    public Pago Pago { get; set; } = null!;
    public int VentaId { get; set; }
    public Venta Venta { get; set; } = null!;
    public decimal MontoAplicado { get; set; }
}
