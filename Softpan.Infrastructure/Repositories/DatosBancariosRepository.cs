using Microsoft.EntityFrameworkCore;
using Softpan.Domain.Entities;
using Softpan.Domain.Interfaces;
using Softpan.Infrastructure.Data;

namespace Softpan.Infrastructure.Repositories;

public class DatosBancariosRepository(ApplicationDbContext context) : IDatosBancariosRepository
{
    public async Task<List<DatosBancarios>> GetAllAsync()
    {
        return await context.Set<DatosBancarios>()
            .OrderByDescending(d => d.Activo)
            .ThenBy(d => d.FechaCreacion)
            .ToListAsync();
    }

    public async Task<DatosBancarios?> GetByIdAsync(int id)
    {
        return await context.Set<DatosBancarios>()
            .FirstOrDefaultAsync(d => d.Id == id);
    }

    public async Task<DatosBancarios?> GetActivoAsync()
    {
        return await context.Set<DatosBancarios>()
            .FirstOrDefaultAsync(d => d.Activo);
    }

    public async Task<DatosBancarios> CreateAsync(DatosBancarios datos)
    {
        context.Set<DatosBancarios>().Add(datos);
        await context.SaveChangesAsync();
        return datos;
    }

    public async Task<DatosBancarios> UpdateAsync(DatosBancarios datos)
    {
        context.Set<DatosBancarios>().Update(datos);
        await context.SaveChangesAsync();
        return datos;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var datos = await context.Set<DatosBancarios>().FindAsync(id);
        if (datos == null) return false;
        context.Set<DatosBancarios>().Remove(datos);
        return await context.SaveChangesAsync() > 0;
    }
}
