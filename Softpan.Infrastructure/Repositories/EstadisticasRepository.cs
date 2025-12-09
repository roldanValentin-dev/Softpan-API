using Microsoft.EntityFrameworkCore;
using Softpan.Domain.Enums;
using Softpan.Domain.Interfaces;
using Softpan.Infrastructure.Data;

namespace Softpan.Infrastructure.Repositories;

public class EstadisticasRepository(ApplicationDbContext context) : IEstadisticasRepository
{
    public async Task<decimal> GetTotalVentasByPeriodoAsync(DateTime fechaInicio, DateTime fechaFin)
    {
        return await context.Ventas
            .Where(v => v.FechaCreacion >= fechaInicio && v.FechaCreacion <= fechaFin)
            .SumAsync(v => v.MontoTotal);
    }

    public async Task<int> GetCantidadTransaccionesByPeriodoAsync(DateTime fechaInicio, DateTime fechaFin)
    {
        return await context.Ventas
            .Where(v => v.FechaCreacion >= fechaInicio && v.FechaCreacion <= fechaFin)
            .CountAsync();
    }

    public async Task<decimal> GetTotalCobradoByPeriodoAsync(DateTime fechaInicio, DateTime fechaFin)
    {
        return await context.Ventas
            .Where(v => v.FechaCreacion >= fechaInicio && v.FechaCreacion <= fechaFin)
            .SumAsync(v => v.MontoPagado);
    }

    public async Task<List<(int ProductoId, string NombreProducto, int CantidadVendida, decimal TotalVendido)>> 
        GetTopProductosVendidosAsync(int top, DateTime? fechaInicio = null, DateTime? fechaFin = null)
    {
        var query = context.DetalleVentas
            .Include(dv => dv.Producto)
            .Include(dv => dv.Venta)
            .AsQueryable();

        if (fechaInicio.HasValue && fechaFin.HasValue)
        {
            query = query.Where(dv => dv.Venta.FechaCreacion >= fechaInicio.Value && 
                                     dv.Venta.FechaCreacion <= fechaFin.Value);
        }

        var resultados = await query
            .GroupBy(dv => new { dv.ProductoId, dv.Producto.Nombre })
            .Select(g => new
            {
                ProductoId = g.Key.ProductoId,
                NombreProducto = g.Key.Nombre,
                CantidadVendida = g.Sum(dv => dv.Cantidad),
                TotalVendido = g.Sum(dv => dv.Cantidad * dv.PrecioUnitario)
            })
            .OrderByDescending(x => x.CantidadVendida)
            .Take(top)
            .ToListAsync();

        return resultados.Select(r => (r.ProductoId, r.NombreProducto, r.CantidadVendida, r.TotalVendido)).ToList();
    }

    public async Task<decimal> GetTotalDeudasPendientesAsync()
    {
        return await context.Ventas
            .Where(v => v.Estado != EstadoVentaEnum.Pagada)
            .SumAsync(v => v.MontoTotal - v.MontoPagado);
    }

    public async Task<int> GetCantidadClientesConDeudaAsync()
    {
        return await context.Ventas
            .Where(v => v.Estado != EstadoVentaEnum.Pagada)
            .Select(v => v.ClienteId)
            .Distinct()
            .CountAsync();
    }

    public async Task<List<(int ClienteId, string NombreCliente, decimal MontoDeuda, int CantidadVentas)>> 
        GetClientesConMayorDeudaAsync(int top)
    {
        var resultados = await context.Ventas
            .Where(v => v.Estado != EstadoVentaEnum.Pagada)
            .Include(v => v.Cliente)
            .GroupBy(v => new { v.ClienteId, v.Cliente.Nombre })
            .Select(g => new
            {
                ClienteId = g.Key.ClienteId,
                NombreCliente = g.Key.Nombre,
                MontoDeuda = g.Sum(v => v.MontoTotal - v.MontoPagado),
                CantidadVentas = g.Count()
            })
            .OrderByDescending(x => x.MontoDeuda)
            .Take(top)
            .ToListAsync();

        return resultados.Select(r => (r.ClienteId, r.NombreCliente, r.MontoDeuda, r.CantidadVentas)).ToList();
    }

    public async Task<List<(string DiaSemana, decimal TotalVentas, int CantidadTransacciones)>> 
        GetVentasPorDiaSemanaAsync(DateTime fechaInicio, DateTime fechaFin)
    {
        var ventas = await context.Ventas
            .Where(v => v.FechaCreacion >= fechaInicio && v.FechaCreacion <= fechaFin)
            .Select(v => new { v.FechaCreacion, v.MontoTotal })
            .ToListAsync();

        return ventas
            .GroupBy(v => v.FechaCreacion.DayOfWeek)
            .Select(g => (
                DiaSemana: GetNombreDia(g.Key),
                TotalVentas: g.Sum(v => v.MontoTotal),
                CantidadTransacciones: g.Count()
            ))
            .OrderBy(x => GetOrdenDia(x.DiaSemana))
            .ToList();
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

    private static int GetOrdenDia(string dia)
    {
        return dia switch
        {
            "Lunes" => 1,
            "Martes" => 2,
            "Miércoles" => 3,
            "Jueves" => 4,
            "Viernes" => 5,
            "Sábado" => 6,
            "Domingo" => 7,
            _ => 8
        };
    }

    public async Task<List<(int TipoCliente, string TipoClienteNombre, decimal TotalVentas, int CantidadTransacciones)>>
        GetVentasPorTipoClienteAsync(DateTime fechaInicio, DateTime fechaFin)
    {
        var resultados = await context.Ventas
            .Where(v => v.FechaCreacion >= fechaInicio && v.FechaCreacion <= fechaFin)
            .Include(v => v.Cliente)
            .GroupBy(v => v.Cliente.TipoCliente)
            .Select(g => new
            {
                TipoCliente = (int)g.Key,
                TotalVentas = g.Sum(v => v.MontoTotal),
                CantidadTransacciones = g.Count()
            })
            .ToListAsync();

        return resultados.Select(r => (
            r.TipoCliente,
            GetNombreTipoCliente(r.TipoCliente),
            r.TotalVentas,
            r.CantidadTransacciones
        )).ToList();
    }

    public async Task<List<(int TipoPago, string TipoPagoNombre, decimal TotalCobrado, int CantidadPagos)>>
        GetMetodosPagoAsync(DateTime fechaInicio, DateTime fechaFin)
    {
        var resultados = await context.Pagos
            .Where(p => p.FechaPago >= fechaInicio && p.FechaPago <= fechaFin)
            .GroupBy(p => p.TipoPago)
            .Select(g => new
            {
                TipoPago = (int)g.Key,
                TotalCobrado = g.Sum(p => p.Monto),
                CantidadPagos = g.Count()
            })
            .ToListAsync();

        return resultados.Select(r => (
            r.TipoPago,
            GetNombreTipoPago(r.TipoPago),
            r.TotalCobrado,
            r.CantidadPagos
        )).ToList();
    }

    public async Task<List<(int ProductoId, string NombreProducto, int DiasSinVenta, DateTime? UltimaVenta)>>
        GetProductosSinMovimientoAsync(int dias)
    {
        var fechaLimite = DateTime.UtcNow.AddDays(-dias);

        var productosConVentas = await context.DetalleVentas
            .Include(dv => dv.Producto)
            .Include(dv => dv.Venta)
            .GroupBy(dv => new { dv.ProductoId, dv.Producto.Nombre })
            .Select(g => new
            {
                ProductoId = g.Key.ProductoId,
                NombreProducto = g.Key.Nombre,
                UltimaVenta = g.Max(dv => dv.Venta.FechaCreacion)
            })
            .Where(p => p.UltimaVenta < fechaLimite)
            .ToListAsync();

        return productosConVentas.Select(p => (
            p.ProductoId,
            p.NombreProducto,
            (int)(DateTime.UtcNow - p.UltimaVenta).TotalDays,
            (DateTime?)p.UltimaVenta
        )).ToList();
    }

    private static string GetNombreTipoCliente(int tipo)
    {
        return tipo switch
        {
            0 => "Común",
            1 => "Comercio",
            2 => "Revendedor",
            _ => "Desconocido"
        };
    }

    private static string GetNombreTipoPago(int tipo)
    {
        return tipo switch
        {
            1 => "Efectivo",
            2 => "Transferencia",
            _ => "Desconocido"
        };
    }

    public async Task<List<(int ProductoId, string NombreProducto, DayOfWeek DiaSemana, decimal PromedioVentas)>>
        GetPromedioVentasPorDiaSemanaAsync()
    {
        var hace60Dias = DateTime.UtcNow.AddDays(-60);

        var ventas = await context.DetalleVentas
            .Include(dv => dv.Producto)
            .Include(dv => dv.Venta)
            .Where(dv => dv.Venta.FechaCreacion >= hace60Dias)
            .Select(dv => new
            {
                dv.ProductoId,
                dv.Producto.Nombre,
                DiaSemana = dv.Venta.FechaCreacion.DayOfWeek,
                dv.Cantidad
            })
            .ToListAsync();

        return ventas
            .GroupBy(v => new { v.ProductoId, v.Nombre, v.DiaSemana })
            .Select(g => (
                ProductoId: g.Key.ProductoId,
                NombreProducto: g.Key.Nombre,
                DiaSemana: g.Key.DiaSemana,
                PromedioVentas: (decimal)g.Average(v => v.Cantidad)
            ))
            .ToList();
    }
}
