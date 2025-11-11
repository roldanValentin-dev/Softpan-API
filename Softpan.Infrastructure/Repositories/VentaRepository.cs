
using Microsoft.EntityFrameworkCore;
using Softpan.Domain.Entities;
using Softpan.Domain.Enums;
using Softpan.Domain.Interfaces;
using Softpan.Infrastructure.Data;

namespace Softpan.Infrastructure.Repositories;

public class VentaRepository(ApplicationDbContext context): IVentaRepository
{

    public async Task<Venta?> GetByIdAsync(int id)
    {
        return await context.Ventas
            .Include(v => v.Cliente)
            .Include(v => v.DetallesVenta)
                .ThenInclude(d => d.Producto)
            .Include(v => v.PagosVenta)
                .ThenInclude(pv => pv.Pago)
            .FirstOrDefaultAsync(v => v.Id == id);
    }
    public async Task<IEnumerable<Venta>> GetAllAsync()
    {
        return await context.Ventas
            .Include(v => v.Cliente)
            .ToListAsync();
    }

    public async Task<Venta> CreateAsync(Venta venta)
    {
        context.Ventas.Add(venta);
        await context.SaveChangesAsync();
        return venta;
    }

    public async Task<Venta> UpdateAsync(Venta venta)
    {
        context.Ventas.Update(venta);
        await context.SaveChangesAsync();
        return venta;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var venta = await context.Ventas.FindAsync(id);
        if (venta == null) return false;

        context.Ventas.Remove(venta);
        return await context.SaveChangesAsync() > 0;
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await context.Ventas.AnyAsync(v => v.Id == id);
    }

    public async Task<IEnumerable<Venta>> GetVentasByClienteAsync(int clienteId)
    {
        return await context.Ventas
            .Include(v => v.DetallesVenta)
            .Where(v => v.ClienteId == clienteId)
            .OrderByDescending(v => v.FechaCreacion)
            .ToListAsync();
    }

    public async Task<IEnumerable<Venta>> GetVentasByEstadoAsync(EstadoVentaEnum estado)
    {
        return await context.Ventas
            .Include(v => v.Cliente)
            .Where(v => v.Estado == estado)
            .OrderByDescending(v => v.FechaCreacion)
            .ToListAsync();
    }

    public async Task<IEnumerable<Venta>> GetVentasPendientesAsync()
    {
        return await context.Ventas
            .Include(v => v.Cliente)
            .Where(v => v.Estado == EstadoVentaEnum.Pendiente ||
                       v.Estado == EstadoVentaEnum.ParcialmentePagada)
            .OrderByDescending(v => v.FechaCreacion)
            .ToListAsync();
    }

    public async Task<IEnumerable<Venta>> GetVentasByFechaAsync(DateTime fechaInicio, DateTime fechaFin)
    {
        return await context.Ventas
            .Include(v => v.Cliente)
            .Where(v => v.FechaCreacion >= fechaInicio && v.FechaCreacion <= fechaFin)
            .OrderByDescending(v => v.FechaCreacion)
            .ToListAsync();
    }

    public async Task<decimal> GetTotalVentasByClienteAsync(int clienteId)
    {
        return await context.Ventas
            .Where(v => v.ClienteId == clienteId)
            .SumAsync(v => v.MontoTotal);
    }
}
