

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Softpan.Domain.Interfaces;
using Softpan.Infrastructure.Data;
using Softpan.Infrastructure.Repositories;

namespace Softpan.Infrastructure;

public static class DependencyInjections
{
    public static IServiceCollection AddInfrastracture( this IServiceCollection services , IConfiguration configuration)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"), b => b.MigrationsAssembly("Softpan.Infrastructure"));
        });

        //repositorios
        services.AddScoped<IClienteRepository, ClienteRepository>();
        services.AddScoped<IVentaRepository, VentaRepository>();
        services.AddScoped<IProductoRepository, ProductoRepository>();
        services.AddScoped<IPagoRepository, PagoRepository>();

        return services;
    }
}
