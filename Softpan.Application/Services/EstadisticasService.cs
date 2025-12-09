using Softpan.Application.DTOs;
using Softpan.Application.Interfaces;
using Softpan.Domain.Interfaces;

namespace Softpan.Application.Services;

public class EstadisticasService(IEstadisticasRepository estadisticasRepository) : IEstadisticasService
{
    public async Task<DashboardDto> GetDashboardAsync()
    {
        var dashboard = new DashboardDto
        {
            VentasHoy = await GetVentasHoyAsync(),
            VentasMes = await GetVentasMesAsync(),
            Deudas = await GetResumenDeudasAsync(),
            TopProductos = await GetTopProductosAsync(5),
            ClientesConMayorDeuda = await GetClientesConMayorDeudaAsync(5),
            ComparativaMensual = await GetComparativaMensualAsync(),
            VentasPorTipoCliente = await GetVentasPorTipoClienteAsync(),
            MetodosPago = await GetMetodosPagoAsync(),
            ProductosSinMovimiento = await GetProductosSinMovimientoAsync(30)
        };

        return dashboard;
    }

    public async Task<VentasResumenDto> GetVentasHoyAsync()
    {
        var hoy = DateTime.UtcNow.Date;
        var manana = hoy.AddDays(1);
        return await GetVentasByPeriodoAsync(hoy, manana);
    }

    public async Task<VentasResumenDto> GetVentasSemanaAsync()
    {
        var hoy = DateTime.UtcNow.Date;
        var inicioSemana = hoy.AddDays(-(int)hoy.DayOfWeek + 1);
        var finSemana = inicioSemana.AddDays(7);
        return await GetVentasByPeriodoAsync(inicioSemana, finSemana);
    }

    public async Task<VentasResumenDto> GetVentasMesAsync()
    {
        var hoy = DateTime.UtcNow.Date;
        var inicioMes = new DateTime(hoy.Year, hoy.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var finMes = inicioMes.AddMonths(1);
        return await GetVentasByPeriodoAsync(inicioMes, finMes);
    }

    public async Task<VentasResumenDto> GetVentasByPeriodoAsync(DateTime fechaInicio, DateTime fechaFin)
    {
        var totalVentas = await estadisticasRepository.GetTotalVentasByPeriodoAsync(fechaInicio, fechaFin);
        var cantidadTransacciones = await estadisticasRepository.GetCantidadTransaccionesByPeriodoAsync(fechaInicio, fechaFin);
        var totalCobrado = await estadisticasRepository.GetTotalCobradoByPeriodoAsync(fechaInicio, fechaFin);

        return new VentasResumenDto
        {
            TotalVentas = totalVentas,
            CantidadTransacciones = cantidadTransacciones,
            TicketPromedio = cantidadTransacciones > 0 ? totalVentas / cantidadTransacciones : 0,
            TotalCobrado = totalCobrado
        };
    }

    public async Task<List<ProductoVentasDto>> GetTopProductosAsync(int top = 5)
    {
        var productos = await estadisticasRepository.GetTopProductosVendidosAsync(top);
        
        return productos.Select(p => new ProductoVentasDto
        {
            ProductoId = p.ProductoId,
            NombreProducto = p.NombreProducto,
            CantidadVendida = p.CantidadVendida,
            TotalVendido = p.TotalVendido
        }).ToList();
    }

    public async Task<List<ProductoVentasDto>> GetTopProductosByPeriodoAsync(int top, DateTime fechaInicio, DateTime fechaFin)
    {
        var productos = await estadisticasRepository.GetTopProductosVendidosAsync(top, fechaInicio, fechaFin);
        
        return productos.Select(p => new ProductoVentasDto
        {
            ProductoId = p.ProductoId,
            NombreProducto = p.NombreProducto,
            CantidadVendida = p.CantidadVendida,
            TotalVendido = p.TotalVendido
        }).ToList();
    }

    public async Task<DeudasResumenDto> GetResumenDeudasAsync()
    {
        var totalDeudas = await estadisticasRepository.GetTotalDeudasPendientesAsync();
        var cantidadClientes = await estadisticasRepository.GetCantidadClientesConDeudaAsync();

        return new DeudasResumenDto
        {
            TotalDeudas = totalDeudas,
            CantidadClientesConDeuda = cantidadClientes,
            PromedioDeudaPorCliente = cantidadClientes > 0 ? totalDeudas / cantidadClientes : 0
        };
    }

    public async Task<List<ClienteDeudaDto>> GetClientesConMayorDeudaAsync(int top = 5)
    {
        var clientes = await estadisticasRepository.GetClientesConMayorDeudaAsync(top);
        
        return clientes.Select(c => new ClienteDeudaDto
        {
            ClienteId = c.ClienteId,
            NombreCliente = c.NombreCliente,
            MontoDeuda = c.MontoDeuda,
            CantidadVentasPendientes = c.CantidadVentas
        }).ToList();
    }

    public async Task<ComparativaVentasDto> GetComparativaMensualAsync()
    {
        var hoy = DateTime.UtcNow.Date;
        var inicioMesActual = new DateTime(hoy.Year, hoy.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var finMesActual = inicioMesActual.AddMonths(1);
        var inicioMesAnterior = inicioMesActual.AddMonths(-1);

        var ventasMesActual = await estadisticasRepository.GetTotalVentasByPeriodoAsync(inicioMesActual, finMesActual);
        var ventasMesAnterior = await estadisticasRepository.GetTotalVentasByPeriodoAsync(inicioMesAnterior, inicioMesActual);

        return CalcularComparativa(ventasMesActual, ventasMesAnterior);
    }

    public async Task<ComparativaVentasDto> GetComparativaSemanalAsync()
    {
        var hoy = DateTime.UtcNow.Date;
        var inicioSemanaActual = hoy.AddDays(-(int)hoy.DayOfWeek + 1);
        var finSemanaActual = inicioSemanaActual.AddDays(7);
        var inicioSemanaAnterior = inicioSemanaActual.AddDays(-7);

        var ventasSemanaActual = await estadisticasRepository.GetTotalVentasByPeriodoAsync(inicioSemanaActual, finSemanaActual);
        var ventasSemanaAnterior = await estadisticasRepository.GetTotalVentasByPeriodoAsync(inicioSemanaAnterior, inicioSemanaActual);

        return CalcularComparativa(ventasSemanaActual, ventasSemanaAnterior);
    }

    public async Task<List<VentasPorDiaDto>> GetVentasPorDiaSemanaAsync()
    {
        var hoy = DateTime.UtcNow.Date;
        var hace30Dias = hoy.AddDays(-30);

        var ventas = await estadisticasRepository.GetVentasPorDiaSemanaAsync(hace30Dias, hoy);
        
        return ventas.Select(v => new VentasPorDiaDto
        {
            DiaSemana = v.DiaSemana,
            TotalVentas = v.TotalVentas,
            CantidadTransacciones = v.CantidadTransacciones
        }).ToList();
    }

    private static ComparativaVentasDto CalcularComparativa(decimal periodoActual, decimal periodoAnterior)
    {
        var diferencia = periodoActual - periodoAnterior;
        var porcentaje = periodoAnterior > 0 ? (diferencia / periodoAnterior) * 100 : 0;

        return new ComparativaVentasDto
        {
            VentasPeriodoActual = periodoActual,
            VentasPeriodoAnterior = periodoAnterior,
            DiferenciaAbsoluta = diferencia,
            PorcentajeCrecimiento = porcentaje
        };
    }

    public async Task<List<VentasPorTipoClienteDto>> GetVentasPorTipoClienteAsync()
    {
        var hoy = DateTime.UtcNow.Date;
        var inicioMes = new DateTime(hoy.Year, hoy.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var finMes = inicioMes.AddMonths(1);

        var ventas = await estadisticasRepository.GetVentasPorTipoClienteAsync(inicioMes, finMes);
        var totalVentas = ventas.Sum(v => v.TotalVentas);

        return ventas.Select(v => new VentasPorTipoClienteDto
        {
            TipoCliente = v.TipoCliente,
            TipoClienteNombre = v.TipoClienteNombre,
            TotalVentas = v.TotalVentas,
            CantidadTransacciones = v.CantidadTransacciones,
            Porcentaje = totalVentas > 0 ? (v.TotalVentas / totalVentas) * 100 : 0
        }).ToList();
    }

    public async Task<List<MetodosPagoDto>> GetMetodosPagoAsync()
    {
        var hoy = DateTime.UtcNow.Date;
        var inicioMes = new DateTime(hoy.Year, hoy.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var finMes = inicioMes.AddMonths(1);

        var pagos = await estadisticasRepository.GetMetodosPagoAsync(inicioMes, finMes);
        var totalCobrado = pagos.Sum(p => p.TotalCobrado);

        return pagos.Select(p => new MetodosPagoDto
        {
            TipoPago = p.TipoPago,
            TipoPagoNombre = p.TipoPagoNombre,
            TotalCobrado = p.TotalCobrado,
            CantidadPagos = p.CantidadPagos,
            Porcentaje = totalCobrado > 0 ? (p.TotalCobrado / totalCobrado) * 100 : 0
        }).ToList();
    }

    public async Task<List<ProductoSinMovimientoDto>> GetProductosSinMovimientoAsync(int dias = 30)
    {
        var productos = await estadisticasRepository.GetProductosSinMovimientoAsync(dias);

        return productos.Select(p => new ProductoSinMovimientoDto
        {
            ProductoId = p.ProductoId,
            NombreProducto = p.NombreProducto,
            DiasSinVenta = p.DiasSinVenta,
            UltimaVenta = p.UltimaVenta
        }).ToList();
    }

    public async Task<List<PrediccionDemandaDto>> GetPrediccionDemandaAsync(DayOfWeek? diaSemana = null)
    {
        var promedios = await estadisticasRepository.GetPromedioVentasPorDiaSemanaAsync();
        var diaObjetivo = diaSemana ?? DateTime.UtcNow.AddDays(1).DayOfWeek;

        var predicciones = promedios
            .Where(p => p.DiaSemana == diaObjetivo)
            .Select(p =>
            {
                var tendencia = CalcularTendencia(p.ProductoId);
                var sugerencia = p.PromedioVentas * (1 + (tendencia / 100));

                return new PrediccionDemandaDto
                {
                    ProductoId = p.ProductoId,
                    NombreProducto = p.NombreProducto,
                    DiaSemana = GetNombreDia(p.DiaSemana),
                    PromedioVentas = Math.Round(p.PromedioVentas, 0),
                    TendenciaCrecimiento = Math.Round(tendencia, 1),
                    SugerenciaProduccion = Math.Round(sugerencia, 0)
                };
            })
            .OrderByDescending(p => p.SugerenciaProduccion)
            .ToList();

        return predicciones;
    }

    private decimal CalcularTendencia(int productoId)
    {
        var hoy = DateTime.UtcNow.Date;
        var hace30Dias = hoy.AddDays(-30);
        var hace60Dias = hoy.AddDays(-60);

        var ventasRecientes = estadisticasRepository.GetTotalVentasByPeriodoAsync(hace30Dias, hoy).Result;
        var ventasAnteriores = estadisticasRepository.GetTotalVentasByPeriodoAsync(hace60Dias, hace30Dias).Result;

        if (ventasAnteriores == 0) return 0;
        return ((ventasRecientes - ventasAnteriores) / ventasAnteriores) * 100;
    }

    private static string GetNombreDia(DayOfWeek dia)
    {
        return dia switch
        {
            DayOfWeek.Monday => "Lunes",
            DayOfWeek.Tuesday => "Martes",
            DayOfWeek.Wednesday => "Miércoles",
            DayOfWeek.Thursday => "Jueves",
            DayOfWeek.Friday => "Viernes",
            DayOfWeek.Saturday => "Sábado",
            DayOfWeek.Sunday => "Domingo",
            _ => "Desconocido"
        };
    }
}
