
using Softpan.Domain.Enums;

namespace Softpan.Domain.Entities;

public class Pago
{
    public int  Id { get; set; }
    public int ClienteId { get; set; }
    public Cliente Cliente { get; set; } = null!;
    public decimal Monto { get; set; }

    public TipoPagoEnum TipoPago { get; set; }
    public DateTime FechaPago { get; set; } = DateTime.UtcNow;
    public string? Observaciones { get; set; }

    public ICollection<PagoVenta> PagosAplicado { get; set; } = new List<PagoVenta>();
}
