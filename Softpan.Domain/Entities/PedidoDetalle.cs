

namespace Softpan.Domain.Entities;

public class PedidoDetalle
{
    public int Id { get; set; } 
    public int PedidoId { get; set; }
    public Pedido Pedido { get; set; } = null!;

    //relacion con producto
    public int ProductoId { get; set; }
    public Producto Producto { get; set; } = null!;

    //datos del detalle
    public int Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }

    //propiedad calculada
    public decimal Subtotal => Cantidad * PrecioUnitario;
}
