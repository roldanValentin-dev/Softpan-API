using Microsoft.EntityFrameworkCore;
using Softpan.Domain.Entities;
using Softpan.Domain.Interfaces;
using Softpan.Infrastructure.Data;

namespace Softpan.Infrastructure.Repositories;

public class ConfiguracionRepository(ApplicationDbContext context) : IConfiguracionRepository
{
    public async Task<ConfiguracionPago?> GetByClaveAsync(string clave)
    {
        return await context.Set<ConfiguracionPago>()
            .FirstOrDefaultAsync(c => c.Clave == clave);
    }

    public async Task<List<ConfiguracionPago>> GetAllAsync()
    {
        return await context.Set<ConfiguracionPago>()
            .ToListAsync();
    }

    public async Task<ConfiguracionPago> CreateAsync(ConfiguracionPago config)
    {
        context.Set<ConfiguracionPago>().Add(config);
        await context.SaveChangesAsync();
        return config;
    }

    public async Task<ConfiguracionPago> UpdateAsync(ConfiguracionPago config)
    {
        config.FechaActualizacion = DateTime.UtcNow;
        context.Set<ConfiguracionPago>().Update(config);
        await context.SaveChangesAsync();
        return config;
    }
}
