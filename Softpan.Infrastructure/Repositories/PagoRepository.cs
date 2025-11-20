
using Microsoft.EntityFrameworkCore;
using Softpan.Domain.Entities;
using Softpan.Domain.Enums;
using Softpan.Domain.Interfaces;
using Softpan.Infrastructure.Data;

namespace Softpan.Infrastructure.Repositories;

public class PagoRepository(ApplicationDbContext context) : IPagoRepository
{
    public async Task<Pago?> GetByIdAsync(int id)
    {
        return await context.Pagos
            .AsNoTracking()
            .Include(p => p.Cliente)
            .Include(p => p.PagosAplicado)
            .ThenInclude(pv => pv.Venta)
            .FirstOrDefaultAsync(p => p.Id == id);
    }
    public async Task<IEnumerable<Pago>> GetAllAsync()
    {
        return await context.Pagos
            .AsNoTracking()
            .Include(p => p.Cliente)
            .ToListAsync();
    }

    public async Task<Pago> CreateAsync(Pago pago)
    {
        context.Add(pago);
        await context.SaveChangesAsync();
        return pago;
    }
    public async Task<Pago> UpdateAsync(Pago pago)
    {
        context.Update(pago);
        await context.SaveChangesAsync();
        return pago;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var pago = await context.Pagos.FindAsync(id);

        if (pago == null) return false;

        context.Remove(pago);

        return await context.SaveChangesAsync() > 0;
    }
    public async Task<bool> ExistsAsync(int id)
    {
        return await context.Pagos.AnyAsync(p => p.Id == id);
    }

    public async Task<IEnumerable<Pago>> GetPagosByClienteAsync(int clienteId)
    {
        return await context.Pagos
            .AsNoTracking()
            .Include(p => p.PagosAplicado)
            .Where(p => p.ClienteId == clienteId)
            .OrderByDescending(p => p.FechaPago)
            .ToListAsync();
    }

    public async Task<IEnumerable<Pago>> GetPagosByTipoAsync(TipoPagoEnum tipo)
    {
        return await context.Pagos
            .AsNoTracking()
            .Include(p => p.Cliente)
            .Where(p => p.TipoPago == tipo)
            .OrderByDescending(p => p.FechaPago)
            .ToListAsync();
    }

    public async Task<IEnumerable<Pago>> GetPagosByFechaAsync(DateTime fechaInicio, DateTime fechaFin)
    {
        return await context.Pagos
            .AsNoTracking()
            .Include(p => p.Cliente)
            .Where(p => p.FechaPago >= fechaInicio && p.FechaPago <= fechaFin)
            .OrderByDescending(p => p.FechaPago)
            .ToListAsync();
    }

    public async Task<decimal> GetTotalPagosByClienteAsync(int clienteId)
    {
        return await context.Pagos
            .Where(p => p.ClienteId == clienteId)
            .SumAsync(p => p.Monto);
    }
}
