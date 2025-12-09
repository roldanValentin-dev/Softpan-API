namespace Softpan.Application.DTOs;

// Resumen de ventas por período
public class VentasResumenDto
{
    public decimal TotalVentas { get; set; }
    public int CantidadTransacciones { get; set; }
    public decimal TicketPromedio { get; set; }
    public decimal TotalCobrado { get; set; }
}

// Producto más vendido
public class ProductoVentasDto
{
    public int ProductoId { get; set; }
    public string NombreProducto { get; set; } = string.Empty;
    public int CantidadVendida { get; set; }
    public decimal TotalVendido { get; set; }
}

// Resumen de deudas
public class DeudasResumenDto
{
    public decimal TotalDeudas { get; set; }
    public int CantidadClientesConDeuda { get; set; }
    public decimal PromedioDeudaPorCliente { get; set; }
}

// Cliente con mayor deuda
public class ClienteDeudaDto
{
    public int ClienteId { get; set; }
    public string NombreCliente { get; set; } = string.Empty;
    public decimal MontoDeuda { get; set; }
    public int CantidadVentasPendientes { get; set; }
}

// Comparativa de ventas
public class ComparativaVentasDto
{
    public decimal VentasPeriodoActual { get; set; }
    public decimal VentasPeriodoAnterior { get; set; }
    public decimal DiferenciaAbsoluta { get; set; }
    public decimal PorcentajeCrecimiento { get; set; }
}

// Ventas por día de la semana
public class VentasPorDiaDto
{
    public string DiaSemana { get; set; } = string.Empty;
    public decimal TotalVentas { get; set; }
    public int CantidadTransacciones { get; set; }
}

// Ventas por tipo de cliente
public class VentasPorTipoClienteDto
{
    public int TipoCliente { get; set; }
    public string TipoClienteNombre { get; set; } = string.Empty;
    public decimal TotalVentas { get; set; }
    public int CantidadTransacciones { get; set; }
    public decimal Porcentaje { get; set; }
}

// Métodos de pago más usados
public class MetodosPagoDto
{
    public int TipoPago { get; set; }
    public string TipoPagoNombre { get; set; } = string.Empty;
    public decimal TotalCobrado { get; set; }
    public int CantidadPagos { get; set; }
    public decimal Porcentaje { get; set; }
}

// Productos sin movimiento
public class ProductoSinMovimientoDto
{
    public int ProductoId { get; set; }
    public string NombreProducto { get; set; } = string.Empty;
    public int DiasSinVenta { get; set; }
    public DateTime? UltimaVenta { get; set; }
}

// Dashboard completo (combina todo)
public class DashboardDto
{
    public VentasResumenDto VentasHoy { get; set; } = new();
    public VentasResumenDto VentasMes { get; set; } = new();
    public DeudasResumenDto Deudas { get; set; } = new();
    public List<ProductoVentasDto> TopProductos { get; set; } = new();
    public List<ClienteDeudaDto> ClientesConMayorDeuda { get; set; } = new();
    public ComparativaVentasDto ComparativaMensual { get; set; } = new();
    public List<VentasPorTipoClienteDto> VentasPorTipoCliente { get; set; } = new();
    public List<MetodosPagoDto> MetodosPago { get; set; } = new();
    public List<ProductoSinMovimientoDto> ProductosSinMovimiento { get; set; } = new();
}
