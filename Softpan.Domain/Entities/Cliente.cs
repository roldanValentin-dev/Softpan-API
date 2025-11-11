using Softpan.Domain.Enums;

namespace Softpan.Domain.Entities;

public class Cliente
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Telefono { get; set; } = string.Empty;
    public string? Direccion { get; set; } = string.Empty;
    public TipoClienteEnum TipoCliente { get; set; }
    public bool Activo { get; set; } = true;
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    public DateTime? FechaModificacion { get; set; }

    public ICollection<Venta> Ventas { get; set; } = new List<Venta>();
    public ICollection<PrecioCliente> PreciosClientes { get; set; } = new List<PrecioCliente>();


    public decimal ObtenerDeudaTotal()
    {
        return Ventas
            .Where(v => v.Estado != EstadoVentaEnum.Pagada)
            .Sum(v => v.MontoTotal - v.MontoPagado);
    }

}
