
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
    public DateTime? FechaCancelacion { get; set; }

    //estaddo y totales

    public EstadoPedidoEnum Estado { get; set; } = EstadoPedidoEnum.Pendiente;
    public bool StockDescontado { get; set; } = false;

    public decimal Total { get; set; }

    public string? Observaciones { get; set; }

    //campos de pago
    public TipoPagoEnum? TipoPago { get; set; }
    public EstadoPagoEnum EstadoPago { get; set; } = EstadoPagoEnum.Pendiente;
    public decimal? MontoConDescuento { get; set; }
    public string? ReferenciaTransaccion { get; set; }
    public DateTime? FechaPago { get; set; }

    //Campos de mercado Pago 
    public string? MercadoPagoPreferenceId { get; set; }
    public string? MercadoPagoPaymentId { get; set; }
    public string? PaymentGateway { get; set; }
    public string? PaymentStatus { get; set; }
    public string? PaymentStatusDetails { get; set; }
    public DateTime? PaymentFechaActualizado { get; set; }


    public ICollection<PedidoDetalle> Detalles { get; set; } = [];

    // Método de negocio
    public void CalcularTotal()
    {
        Total = Detalles.Sum(d => d.Subtotal);
    }

    public bool PuedeCancelarse() => Estado == EstadoPedidoEnum.Pendiente;

    public bool EsPagoConfirmado () => EstadoPago == EstadoPagoEnum.Pagado;
}
