
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Softpan.Application.Interfaces;
using Softpan.Domain.Interfaces;
using Softpan.Infrastructure.Data;
using Softpan.Infrastructure.Repositories;
using Softpan.Infrastructure.Services;

namespace Softpan.Infrastructure;

public static class DependencyInjections
{
    public static IServiceCollection AddInfrastracture( this IServiceCollection services , IConfiguration configuration)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection"), b => b.MigrationsAssembly("Softpan.Infrastructure"));
        });

        //Redis
        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = configuration.GetConnectionString("Redis");
        });

        services.AddScoped<IRedisCacheService, RedisCacheService>();

        // Unit of Work (para transacciones)
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        //repositorios
        services.AddScoped<IClienteRepository, ClienteRepository>();
        services.AddScoped<IVentaRepository, VentaRepository>();
        services.AddScoped<IProductoRepository, ProductoRepository>();
        services.AddScoped<IPagoRepository, PagoRepository>();
        services.AddScoped<IEstadisticasRepository, EstadisticasRepository>();

        return services;
    }
}
