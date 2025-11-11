
using Mapster;
using Softpan.Application.DTOs;
using Softpan.Domain.Entities;
using Softpan.Domain.Enums;

namespace Softpan.Application.Mapping;

public static class MappingConfig
{
    public static void RegisterMappings()
    {
        var config = TypeAdapterConfig.GlobalSettings;


        //Cliente mappings
        config.NewConfig<Cliente, ClienteDto>()
            .Map(dest => dest.TipoClienteNombre, src => ((TipoClienteEnum)src.TipoCliente).ToString());
        config.NewConfig<CreateClienteDto, Cliente>()
            .Map(dest => dest.Activo, src => true)
            .Map(dest => dest.FechaCreacion, src => DateTime.UtcNow);

        config.NewConfig<UpdateClienteDto, Cliente>()
            .Map(dest => dest.FechaModificacion, src => DateTime.UtcNow);


        // Venta mappings
        config.NewConfig<Venta, VentaDto>()
            .Map(dest => dest.ClienteNombre, src => src.Cliente.Nombre)
            .Map(dest => dest.SaldoPendiente, src => src.ObtenerSaldoPendiente())
            .Map(dest => dest.EstadoNombre, src => src.Estado.ToString())
            .Map(dest => dest.Detalles, src => src.DetallesVenta);

        config.NewConfig<DetalleVenta, DetalleVentaDto>()
            .Map(dest => dest.ProductoNombre, src => src.Producto.Nombre);

        config.NewConfig<CreateVentaDto, Venta>()
            .Map(dest => dest.FechaCreacion, src => DateTime.UtcNow)
            .Map(dest => dest.Estado, src => EstadoVentaEnum.Pendiente)
            .Map(dest => dest.MontoPagado, src => 0)
            .Map(dest => dest.DetallesVenta, src => src.Detalles);

        config.NewConfig<CreateDetalleVentaDto, DetalleVenta>();


        // Producto mappings
        config.NewConfig<Producto, ProductoDto>();

        config.NewConfig<Producto, ProductoDetalleDto>()
            .Map(dest => dest.PreciosPersonalizados, src => src.PreciosPersonalizados);

        config.NewConfig<PrecioCliente, PrecioPersonalizadoDto>()
            .Map(dest => dest.ClienteNombre, src => src.Cliente.Nombre);

        config.NewConfig<CreateProductoDto, Producto>()
            .Map(dest => dest.Activo, src => true)
            .Map(dest => dest.FechaCreacion, src => DateTime.UtcNow);

        config.NewConfig<UpdateProductoDto, Producto>()
            .Map(dest => dest.FechaModificacion, src => DateTime.UtcNow);

        // Pago mappings
        config.NewConfig<Pago, PagoDto>()
            .Map(dest => dest.ClienteNombre, src => src.Cliente.Nombre)
            .Map(dest => dest.TipoPagoNombre, src => src.TipoPago.ToString());

        config.NewConfig<Pago, PagoDetalleDto>()
            .Map(dest => dest.ClienteNombre, src => src.Cliente.Nombre)
            .Map(dest => dest.TipoPagoNombre, src => src.TipoPago.ToString())
            .Map(dest => dest.PagosAplicados, src => src.PagosAplicado);

        config.NewConfig<PagoVenta, PagoAplicadoDto>();

        config.NewConfig<CreatePagoDto, Pago>()
            .Map(dest => dest.FechaPago, src => DateTime.UtcNow)
            .Ignore(dest => dest.PagosAplicado);

        config.Compile();
    }
}
