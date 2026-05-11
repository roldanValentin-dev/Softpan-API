
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Softpan.Application.DTOs;
using Softpan.Application.Interfaces;
using Softpan.Application.Mapping;
using Softpan.Application.Services;
using Softpan.Application.Validators;

namespace Softpan.Application;

public static class DependencyInjections
{

    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        //Mapster
        MappingConfig.RegisterMappings();


        //Servicios
        services.AddScoped<IClienteService, ClienteService>();
        services.AddScoped<IVentaService, VentaService>();
        services.AddScoped<IProductoService, ProductoService>();
        services.AddScoped<IPagoService, PagoService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IEstadisticasService, EstadisticasService>();
        services.AddScoped<IClienteOnlineService, ClienteOnlineService>();
        services.AddScoped<IPedidoService, PedidoService>();
        services.AddScoped<IProductoImagenService, ProductoImagenService>();
        services.AddScoped<IAuditService, AuditService>();

        //Validators
        services.AddScoped<IValidator<CreateClienteDto>, CreateClienteValidator>();
        services.AddScoped<IValidator<UpdateClienteDto>, UpdateClienteValidator>();
        services.AddScoped<IValidator<CreateVentaDto>, CreateVentaValidator>();
        services.AddScoped<IValidator<CreateProductoDto>, CreateProductoValidator>();
        services.AddScoped<IValidator<UpdateProductoDto>, UpdateProductoValidator>();
        services.AddScoped<IValidator<CreatePagoDto>, CreatePagoValidator>();
        services.AddScoped<IValidator<LoginDto>, LoginDtoValidator>();
        services.AddScoped<IValidator<RegisterDto>, RegisterDtoValidator>();
        services.AddScoped<IValidator<RegisterClienteOnlineDto>, RegisterClienteOnlineDtoValidator>();
        services.AddScoped<IValidator<RefreshTokenDto>, RefreshTokenDtoValidator>();
        services.AddScoped<IValidator<CreatePedidoDto>, CreatePedidoValidator>();
        services.AddScoped<IValidator<ProcesarCheckoutDto>, ProcesarCheckoutValidator>();
        services.AddScoped<IValidator<CreateProductoImagenDto>, CreateProductoImagenValidator>();
        services.AddScoped<IValidator<UpdateProductoImagenDto>, UpdateProductoImagenValidator>();
        services.AddScoped<IValidator<UpdateClienteOnlineDto>, UpdateClienteOnlineDtoValidator>();
        return services;
    }
}
