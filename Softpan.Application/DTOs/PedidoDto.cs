namespace Softpan.Application.DTOs;

public class PedidoDto
{
    public int Id { get; set; }
    public int ClienteOnlineId { get; set; }
    public string ClienteNombre { get; set; } = string.Empty;
    public string ClienteEmail { get; set; } = string.Empty;
    public string? ClienteTelefono { get; set; }
    public DateTime FechaPedido { get; set; }
    public DateTime FechaEntrega { get; set; }
    public string Estado { get; set; } = string.Empty; // "Pendiente", "Confirmado", etc.
    public int EstadoId { get; set; }
    public decimal Total { get; set; }
    public string? Observaciones { get; set; }
    public List<PedidoDetalleDto> Detalles { get; set; } = [];
}
// DTO para detalle del pedido
public class PedidoDetalleDto
{
    public int Id { get; set; }
    public int ProductoId { get; set; }
    public string ProductoNombre { get; set; } = string.Empty;
    public string? ProductoImagen { get; set; }
    public string? ProductoCategoria { get; set; }
    public int Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
    public decimal Subtotal { get; set; }
}

// DTO para crear pedido
public class CreatePedidoDto
{
    public DateTime FechaEntrega { get; set; }
    public string? Observaciones { get; set; }
    public List<CreatePedidoDetalleDto> Detalles { get; set; } = [];
}

// DTO para detalle al crear pedido
public class CreatePedidoDetalleDto
{
    public int ProductoId { get; set; }
    public int Cantidad { get; set; }
}

// DTO para cambiar estado del pedido (Admin)
public class UpdateEstadoPedidoDto
{
    public int EstadoId { get; set; } // 1=Pendiente, 2=Confirmado, etc.
}

// DTO para lista resumida de pedidos
public class PedidoResumenDto
{
    public int Id { get; set; }
    public string ClienteNombre { get; set; } = string.Empty;
    public DateTime FechaPedido { get; set; }
    public DateTime FechaEntrega { get; set; }
    public string Estado { get; set; } = string.Empty;
    public decimal Total { get; set; }
    public int CantidadProductos { get; set; }
}
