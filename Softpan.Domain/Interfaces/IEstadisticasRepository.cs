namespace Softpan.Domain.Interfaces;

public interface IEstadisticasRepository
{
    // Ventas por período
    Task<decimal> GetTotalVentasByPeriodoAsync(DateTime fechaInicio, DateTime fechaFin);
    Task<int> GetCantidadTransaccionesByPeriodoAsync(DateTime fechaInicio, DateTime fechaFin);
    Task<decimal> GetTotalCobradoByPeriodoAsync(DateTime fechaInicio, DateTime fechaFin);

    // Productos más vendidos
    Task<List<(int ProductoId, string NombreProducto, int CantidadVendida, decimal TotalVendido)>> 
        GetTopProductosVendidosAsync(int top, DateTime? fechaInicio = null, DateTime? fechaFin = null);

    // Deudas
    Task<decimal> GetTotalDeudasPendientesAsync();
    Task<int> GetCantidadClientesConDeudaAsync();
    Task<List<(int ClienteId, string NombreCliente, decimal MontoDeuda, int CantidadVentas)>> 
        GetClientesConMayorDeudaAsync(int top);

    // Ventas por día de la semana
    Task<List<(string DiaSemana, decimal TotalVentas, int CantidadTransacciones)>> 
        GetVentasPorDiaSemanaAsync(DateTime fechaInicio, DateTime fechaFin);

    // Ventas por tipo de cliente
    Task<List<(int TipoCliente, string TipoClienteNombre, decimal TotalVentas, int CantidadTransacciones)>>
        GetVentasPorTipoClienteAsync(DateTime fechaInicio, DateTime fechaFin);

    // Métodos de pago más usados
    Task<List<(int TipoPago, string TipoPagoNombre, decimal TotalCobrado, int CantidadPagos)>>
        GetMetodosPagoAsync(DateTime fechaInicio, DateTime fechaFin);

    // Productos sin movimiento
    Task<List<(int ProductoId, string NombreProducto, int DiasSinVenta, DateTime? UltimaVenta)>>
        GetProductosSinMovimientoAsync(int dias);

    // Predicciones
    Task<List<(int ProductoId, string NombreProducto, DayOfWeek DiaSemana, decimal PromedioVentas)>>
        GetPromedioVentasPorDiaSemanaAsync();
}
