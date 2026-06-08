using Microsoft.EntityFrameworkCore;
using Softpan.Domain.Entities;
using Softpan.Domain.Interfaces;
using Softpan.Infrastructure.Data;

namespace Softpan.Infrastructure.Repositories;

public class DireccionRetiroRepository(ApplicationDbContext context) : IDireccionRetiroRepository
{
    public async Task<DireccionRetiro?> GetActivaAsync()
    {
        return await context.Set<DireccionRetiro>()
            .FirstOrDefaultAsync(d => d.Activo);
    }

    public async Task<DireccionRetiro> CreateAsync(DireccionRetiro direccion)
    {
        context.Set<DireccionRetiro>().Add(direccion);
        await context.SaveChangesAsync();
        return direccion;
    }

    public async Task<DireccionRetiro> UpdateAsync(DireccionRetiro direccion)
    {
        context.Set<DireccionRetiro>().Update(direccion);
        await context.SaveChangesAsync();
        return direccion;
    }
}
