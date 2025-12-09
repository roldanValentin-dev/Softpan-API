using Softpan.Application.DTOs;

namespace Softpan.Application.Interfaces;

public interface IEstadisticasService
{
    // Dashboard completo
    Task<DashboardDto> GetDashboardAsync();

    // Ventas por período
    Task<VentasResumenDto> GetVentasHoyAsync();
    Task<VentasResumenDto> GetVentasSemanaAsync();
    Task<VentasResumenDto> GetVentasMesAsync();
    Task<VentasResumenDto> GetVentasByPeriodoAsync(DateTime fechaInicio, DateTime fechaFin);

    // Productos
    Task<List<ProductoVentasDto>> GetTopProductosAsync(int top = 5);
    Task<List<ProductoVentasDto>> GetTopProductosByPeriodoAsync(int top, DateTime fechaInicio, DateTime fechaFin);

    // Deudas
    Task<DeudasResumenDto> GetResumenDeudasAsync();
    Task<List<ClienteDeudaDto>> GetClientesConMayorDeudaAsync(int top = 5);

    // Comparativas
    Task<ComparativaVentasDto> GetComparativaMensualAsync();
    Task<ComparativaVentasDto> GetComparativaSemanalAsync();

    // Tendencias
    Task<List<VentasPorDiaDto>> GetVentasPorDiaSemanaAsync();

    // Nuevas estadísticas
    Task<List<VentasPorTipoClienteDto>> GetVentasPorTipoClienteAsync();
    Task<List<MetodosPagoDto>> GetMetodosPagoAsync();
    Task<List<ProductoSinMovimientoDto>> GetProductosSinMovimientoAsync(int dias = 30);

    // Predicciones
    Task<List<PrediccionDemandaDto>> GetPrediccionDemandaAsync(DayOfWeek? diaSemana = null);
}
