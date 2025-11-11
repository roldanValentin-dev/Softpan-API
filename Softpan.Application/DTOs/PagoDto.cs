using Softpan.Domain.Enums;

namespace Softpan.Application.DTOs;

// DTO para respuesta - Información básica del pago
public class PagoDto
{
    public int Id { get; set; }
    public int ClienteId { get; set; }
    public string ClienteNombre { get; set; } = string.Empty;
    public decimal Monto { get; set; }
    public TipoPagoEnum TipoPago { get; set; }
    public string TipoPagoNombre { get; set; } = string.Empty;
    public DateTime FechaPago { get; set; }
    public string? Observaciones { get; set; }
}

// DTO para respuesta detallada - Incluye ventas a las que se aplicó
public class PagoDetalleDto
{
    public int Id { get; set; }
    public int ClienteId { get; set; }
    public string ClienteNombre { get; set; } = string.Empty;
    public decimal Monto { get; set; }
    public TipoPagoEnum TipoPago { get; set; }
    public string TipoPagoNombre { get; set; } = string.Empty;
    public DateTime FechaPago { get; set; }
    public string? Observaciones { get; set; }
    public List<PagoAplicadoDto> PagosAplicados { get; set; } = new();
}

// DTO para pagos aplicados a ventas
public class PagoAplicadoDto
{
    public int Id { get; set; }
    public int VentaId { get; set; }
    public decimal MontoAplicado { get; set; }
}

// DTO para crear pago y aplicarlo a ventas
public class CreatePagoDto
{
    public int ClienteId { get; set; }
    public decimal Monto { get; set; }
    public TipoPagoEnum TipoPago { get; set; }
    public string? Observaciones { get; set; }
    public List<AplicarPagoVentaDto> VentasAAplicar { get; set; } = new();
}

// DTO para especificar a qué venta y cuánto aplicar
public class AplicarPagoVentaDto
{
    public int VentaId { get; set; }
    public decimal MontoAplicado { get; set; }
}
