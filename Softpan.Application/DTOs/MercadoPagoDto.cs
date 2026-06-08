
namespace Softpan.Application.DTOs;

public class MercadoPagoDto
{
    // Request: lo que envía el frontend para crear una preferencia
    public class MercadoPagoPreferenceRequestDto
    {
        public string? EmailPagador { get; set; }
    }
    // Response: lo que devolvemos al frontend después de crear la preferencia
    public class MercadoPagoPreferenceResponseDto
    {
        public string PreferenceId { get; set; } = string.Empty;
        public string InitPoint { get; set; } = string.Empty;
        public int PedidoId { get; set; }
    }
    // Response: resultado de procesar un webhook
    public class PagoResultadoDto
    {
        public bool Exitoso { get; set; }
        public string Mensaje { get; set; } = string.Empty;
        public int? PedidoId { get; set; }
        public string? PaymentId { get; set; }
    }
    // Response: consultar estado de un pago
    public class EstadoPagoDto
    {
        public string Estado { get; set; } = string.Empty;
        public string? Detalle { get; set; }
        public DateTime? FechaActualizacion { get; set; }
    }
}
