
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

        services.AddScoped<IRedisCacheService, NoOpRedisCacheService>();

        // Servicio de almacenamiento de archivos
        services.AddScoped<IFileStorageService, LocalFileStorageService>();

        // Unit of Work (para transacciones)
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        //repositorios
        services.AddScoped<IClienteRepository, ClienteRepository>();
        services.AddScoped<IVentaRepository, VentaRepository>();
        services.AddScoped<IProductoRepository, ProductoRepository>();
        services.AddScoped<IPagoRepository, PagoRepository>();
        services.AddScoped<IEstadisticasRepository, EstadisticasRepository>();

        services.AddScoped<IClienteOnlineRepository, ClientesOnlineRepository>();
        services.AddScoped<IPedidoRepository, PedidoRepository>();
        services.AddScoped<IProductoImagenRepository, ProductoImagenRepository>();
        services.AddScoped<IAuditRepository, AuditRepository>();
        services.AddScoped<IConfiguracionRepository, ConfiguracionRepository>();
        services.AddScoped<IDatosBancariosRepository, DatosBancariosRepository>();
        services.AddScoped<IDireccionRetiroRepository, DireccionRetiroRepository>();

        // ====================================================================
        // SEGURIDAD: HttpClientFactory con políticas de timeout y retry
        // ====================================================================
        // IHttpClientFactory evita agotar sockets (socket exhaustion).
        // El cliente "MercadoPago" se reusa automáticamente con pooling,
        // tiene timeout de 15s y no sigue redirects por seguridad.
        services.AddHttpClient("MercadoPago", client =>
        {
            client.Timeout = TimeSpan.FromSeconds(15);
            client.DefaultRequestHeaders.Add("User-Agent", "Softpan-API/1.0");
        });

        // Servicios externos
        services.AddScoped<IMercadoPagoService, MercadoPagoService>();

        return services;
    }
}
