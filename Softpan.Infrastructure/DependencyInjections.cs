
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

        // Redis (opcional)
        var redisConnection = configuration["Redis:ConnectionString"];
        if (!string.IsNullOrEmpty(redisConnection))
        {
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = redisConnection;
                options.InstanceName = configuration["Redis:InstanceName"];
            });
            services.AddScoped<IRedisCacheService, RedisCacheService>();
        }
        else
        {
            services.AddScoped<IRedisCacheService, NoOpRedisCacheService>();
        }

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
