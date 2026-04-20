
using Softpan.Domain.Enums;

namespace Softpan.Domain.Entities;

public class Pedido
{
    public int Id { get; set; }
    public int ClienteOnlineId { get; set; }
    public ClienteOnline ClienteOnline { get; set; } = null!;

    //fechas
    public DateTime FechaPedido { get; set; } = DateTime.UtcNow;
    public DateTime FechaEntrega { get; set; }

    //estaddo y totales

    public EstadoPedidoEnum Estado { get; set; } = EstadoPedidoEnum.Pendiente;

    public decimal Total { get; set; }

    public string? Observaciones { get; set; }

    public ICollection<PedidoDetalle> Detalles { get; set; } = new List<PedidoDetalle>();

    // Método de negocio
    public void CalcularTotal()
    {
        Total = Detalles.Sum(d => d.Subtotal);
    }
}
