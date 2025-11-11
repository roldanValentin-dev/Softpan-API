
using Softpan.Domain.Enums;

namespace Softpan.Domain.Entities;

public class Venta
{
    public int Id { get; set; }
    public int ClienteId { get; set; }
    public Cliente Cliente { get; set; } = null!;
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    public DateTime? FechaModificacion { get; set; }

    public decimal MontoTotal { get; set; }
    public decimal MontoPagado { get; set; }
    public EstadoVentaEnum Estado { get; set; } = EstadoVentaEnum.Pendiente;

    public ICollection<DetalleVenta> DetallesVenta { get; set; } = new List<DetalleVenta>();

    public ICollection<PagoVenta> PagosVenta { get; set; } = new List<PagoVenta>();


    public void CalcularMontoTotal()
    {
        MontoTotal = DetallesVenta.Sum(d => d.Subtotal);
    }
    public void ActualizarEstado()
    {
        if (MontoPagado == 0)
            Estado = EstadoVentaEnum.Pendiente;
        else if (MontoPagado >= MontoTotal)
            Estado = EstadoVentaEnum.Pagada;
        else
            Estado = EstadoVentaEnum.ParcialmentePagada;
    }
    public decimal ObtenerSaldoPendiente()
    {
        return MontoTotal - MontoPagado;
    }
    public void ActualizarMontoPagado()
    {
        MontoPagado = PagosVenta.Sum(p => p.MontoAplicado);
    }
}
