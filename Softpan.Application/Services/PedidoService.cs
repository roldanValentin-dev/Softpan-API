using Mapster;
using Softpan.Application.DTOs;
using Softpan.Application.Exceptions;
using Softpan.Application.Interfaces;
using Softpan.Domain.Entities;
using Softpan.Domain.Enums;
using Softpan.Domain.Interfaces;

namespace Softpan.Application.Services;

public class PedidoService(
    IPedidoRepository pedidoRepository,
    IClienteOnlineRepository clienteOnlineRepository,
    IProductoRepository productoRepository,
    IConfiguracionRepository configuracionRepository,
    IDatosBancariosRepository datosBancariosRepository,
    IDireccionRetiroRepository direccionRetiroRepository) : IPedidoService
{
    // ========== MÉTODOS PARA CLIENTE ==========

    public async Task<PedidoDto> CreatePedidoAsync(string usuarioIdentityId, CreatePedidoDto dto)
    {
        var cliente = await clienteOnlineRepository.GetByUsuarioIdentityIdAsync(usuarioIdentityId);
        if (cliente == null)
            throw new NotFoundException("Cliente no encontrado");

        if (dto.Detalles == null || !dto.Detalles.Any())
            throw new BadRequestException("El pedido debe tener al menos un producto");

        if (dto.FechaEntrega.Date < DateTime.UtcNow.Date)
            throw new BadRequestException("La fecha de entrega no puede ser anterior a hoy");

        if (dto.Observaciones?.Length > 500)
            throw new BadRequestException("Las observaciones no pueden exceder 500 caracteres");

        var pedido = new Pedido
        {
            ClienteOnlineId = cliente.Id,
            FechaPedido = DateTime.UtcNow,
            FechaEntrega = DateTime.SpecifyKind(dto.FechaEntrega, DateTimeKind.Utc),
            Estado = EstadoPedidoEnum.Pendiente,
            Observaciones = dto.Observaciones,
            Detalles = new List<PedidoDetalle>()
        };

        foreach (var detalleDto in dto.Detalles)
        {
            var producto = await productoRepository.GetByIdAsync(detalleDto.ProductoId);
            if (producto == null)
                throw new NotFoundException($"Producto con ID {detalleDto.ProductoId} no encontrado");

            if (!producto.Activo)
                throw new BadRequestException($"El producto {producto.Nombre} no está disponible");

            if (detalleDto.Cantidad <= 0)
                throw new BadRequestException("La cantidad debe ser mayor a 0");

            // Validar stock disponible (se descuenta solo al confirmar el pedido)
            if (!producto.TieneStock(detalleDto.Cantidad))
                throw new BadRequestException($"Stock insuficiente para {producto.Nombre}. Disponible: {producto.Stock}, Solicitado: {detalleDto.Cantidad}");

            var detalle = new PedidoDetalle
            {
                ProductoId = producto.Id,
                Cantidad = detalleDto.Cantidad,
                PrecioUnitario = producto.PrecioBase
            };

            pedido.Detalles.Add(detalle);
        }

        pedido.CalcularTotal();

        if (dto.TipoPago.HasValue)
        {
            pedido.TipoPago = dto.TipoPago.Value;
            if (dto.TipoPago == TipoPagoEnum.Efectivo || dto.TipoPago == TipoPagoEnum.Transferencia)
            {
                var config = await configuracionRepository.GetByClaveAsync("DescuentoEfectivoTransferencia");
                var porcentaje = config != null ? decimal.Parse(config.Valor) : 10m;
                pedido.MontoConDescuento = pedido.Total * (100 - porcentaje) / 100;
            }
        }

        var pedidoCreado = await pedidoRepository.CreateAsync(pedido);
        var pedidoCompleto = await pedidoRepository.GetByIdWithDetallesAsync(pedidoCreado.Id);

        return MapToDto(pedidoCompleto!);
    }

    public async Task<List<PedidoDto>> GetMisPedidosAsync(string usuarioIdentityId)
    {
        var cliente = await clienteOnlineRepository.GetByUsuarioIdentityIdAsync(usuarioIdentityId);
        if (cliente == null)
            throw new NotFoundException("Cliente no encontrado");

        var pedidos = await pedidoRepository.GetByClienteIdAsync(cliente.Id);
        return pedidos.Where(p => p.Estado != EstadoPedidoEnum.Carrito).Select(MapToDto).ToList();
    }

    public async Task<PedidoDto> GetPedidoByIdAsync(int id, string usuarioIdentityId)
    {
        var cliente = await clienteOnlineRepository.GetByUsuarioIdentityIdAsync(usuarioIdentityId);
        if (cliente == null)
            throw new NotFoundException("Cliente no encontrado");

        var pedido = await pedidoRepository.GetByIdWithDetallesAsync(id);
        if (pedido == null)
            throw new NotFoundException("Pedido", id);

        if (pedido.ClienteOnlineId != cliente.Id)
            throw new UnauthorizedException("No tiene permiso para ver este pedido");

        return MapToDto(pedido);
    }

    public async Task<PedidoDto> CancelarPedidoAsync(int id, string usuarioIdentityId)
    {
        var cliente = await clienteOnlineRepository.GetByUsuarioIdentityIdAsync(usuarioIdentityId);
        if (cliente == null)
            throw new NotFoundException("Cliente no encontrado");

        var pedido = await pedidoRepository.GetByIdWithDetallesAsync(id);
        if (pedido == null)
            throw new NotFoundException("Pedido", id);

        if (pedido.ClienteOnlineId != cliente.Id)
            throw new UnauthorizedException("No tiene permiso para cancelar este pedido");

        if (!pedido.PuedeCancelarse())
            throw new BadRequestException($"No se puede cancelar un pedido en estado {pedido.Estado}");

        // Si el stock ya fue descontado, restaurarlo
        // Usa detalle.Producto (ya trackeado por Include) para evitar conflictos de tracking
        if (pedido.StockDescontado)
        {
            foreach (var detalle in pedido.Detalles)
            {
                detalle.Producto?.RestaurarStock(detalle.Cantidad);
            }
        }

        pedido.Estado = EstadoPedidoEnum.Cancelado;
        pedido.FechaCancelacion = DateTime.UtcNow;

        var pedidoActualizado = await pedidoRepository.UpdateAsync(pedido);
        return MapToDto(pedidoActualizado);
    }

    // ========== MÉTODOS PARA ADMIN ==========

    public async Task<List<PedidoResumenDto>> GetAllPedidosAsync()
    {
        var pedidos = await pedidoRepository.GetAllAsync();
        return pedidos.Where(p => p.Estado != EstadoPedidoEnum.Carrito).Select(MapToResumenDto).ToList();
    }

    public async Task<List<PedidoResumenDto>> GetPedidosByEstadoAsync(EstadoPedidoEnum estado)
    {
        var pedidos = await pedidoRepository.GetByEstadoAsync(estado);
        return pedidos.Select(MapToResumenDto).ToList();
    }

    public async Task<PedidoDto> GetPedidoDetalleByIdAsync(int id)
    {
        var pedido = await pedidoRepository.GetByIdWithDetallesAsync(id);
        if (pedido == null)
            throw new NotFoundException("Pedido", id);

        return MapToDto(pedido);
    }

    public async Task<PedidoDto> UpdateEstadoPedidoAsync(int id, UpdateEstadoPedidoDto dto)
    {
        var pedido = await pedidoRepository.GetByIdWithDetallesAsync(id);
        if (pedido == null)
            throw new NotFoundException("Pedido", id);

        if (!Enum.IsDefined(typeof(EstadoPedidoEnum), dto.EstadoId))
            throw new BadRequestException("Estado de pedido inválido");

        var estadoAnterior = pedido.Estado;
        var estadoNuevo = (EstadoPedidoEnum)dto.EstadoId;

        if (estadoAnterior == EstadoPedidoEnum.Carrito || estadoNuevo == EstadoPedidoEnum.Carrito)
            throw new BadRequestException("No se puede cambiar el estado de un carrito mediante esta operación");

        if (estadoNuevo == EstadoPedidoEnum.Confirmado && pedido.EstadoPago != EstadoPagoEnum.Pagado)
            throw new BadRequestException("No se puede confirmar un pedido sin pago confirmado");

        // Si se confirma el pedido, descontar stock
        // Usa detalle.Producto (ya trackeado por Include) para evitar conflictos de tracking
        if (estadoAnterior == EstadoPedidoEnum.Pendiente && estadoNuevo == EstadoPedidoEnum.Confirmado)
        {
            foreach (var detalle in pedido.Detalles)
            {
                detalle.Producto?.DescontarStock(detalle.Cantidad);
            }
            pedido.StockDescontado = true;
        }

        // Si se cancela desde admin, restaurar stock si fue descontado
        if (estadoNuevo == EstadoPedidoEnum.Cancelado && pedido.StockDescontado)
        {
            foreach (var detalle in pedido.Detalles)
            {
                detalle.Producto?.RestaurarStock(detalle.Cantidad);
            }
            pedido.FechaCancelacion = DateTime.UtcNow;
        }

        pedido.Estado = estadoNuevo;

        var pedidoActualizado = await pedidoRepository.UpdateAsync(pedido);
        return MapToDto(pedidoActualizado);
    }

    //Carrito
    public async Task<CarritoDto> ObtenerOCrearCarritoAsync(string usuarioId)
    {
        var cliente = await clienteOnlineRepository.GetByUsuarioIdentityIdAsync(usuarioId);
        if (cliente == null)
            throw new NotFoundException("Cliente no encontrado");
        var carrito = await pedidoRepository.GetCarritoByClienteIdAsync(cliente.Id);
        if (carrito == null)
        {
            carrito = new Pedido
            {
                ClienteOnlineId = cliente.Id,
                FechaPedido = DateTime.UtcNow,
                Estado = EstadoPedidoEnum.Carrito,
                Detalles = new List<PedidoDetalle>()
            };
            carrito = await pedidoRepository.CreateAsync(carrito);
        }
        return MapToCarritoDto(carrito);
    }
    public async Task<CarritoDto> AgregarItemAlCarritoAsync(string usuarioId, int productoId, int cantidad)
    {
        var cliente = await clienteOnlineRepository.GetByUsuarioIdentityIdAsync(usuarioId);
        if (cliente == null)
            throw new NotFoundException("Cliente no encontrado");
        if (cantidad <= 0)
            throw new BadRequestException("La cantidad debe ser mayor a 0");
        var producto = await productoRepository.GetByIdAsync(productoId);
        if (producto == null)
            throw new NotFoundException("Producto", productoId);
        if (!producto.Activo)
            throw new BadRequestException($"El producto {producto.Nombre} no está disponible");
        if (!producto.TieneStock(cantidad))
            throw new BadRequestException($"Stock insuficiente para {producto.Nombre}. Disponible: {producto.Stock}, Solicitado: {cantidad}");
        var carrito = await pedidoRepository.GetCarritoByClienteIdAsync(cliente.Id);
        if (carrito == null)
        {
            carrito = new Pedido
            {
                ClienteOnlineId = cliente.Id,
                FechaPedido = DateTime.UtcNow,
                Estado = EstadoPedidoEnum.Carrito,
                Detalles = new List<PedidoDetalle>()
            };
            carrito = await pedidoRepository.CreateAsync(carrito);
        }
        var detalleExistente = carrito.Detalles.FirstOrDefault(d => d.ProductoId == productoId);
        if (detalleExistente != null)
        {
            var nuevaCantidad = detalleExistente.Cantidad + cantidad;
            if (!producto.TieneStock(nuevaCantidad))
                throw new BadRequestException($"Stock insuficiente. Ya tienes {detalleExistente.Cantidad} en tu carrito. Disponible: {producto.Stock}");
            detalleExistente.Cantidad = nuevaCantidad;
        }
        else
        {
            carrito.Detalles.Add(new PedidoDetalle
            {
                ProductoId = producto.Id,
                Cantidad = cantidad,
                PrecioUnitario = producto.PrecioBase
            });
        }
        carrito.CalcularTotal();
        await pedidoRepository.UpdateAsync(carrito);
        carrito = await pedidoRepository.GetCarritoByClienteIdAsync(cliente.Id);
        return MapToCarritoDto(carrito!);
    }
    public async Task<CarritoDto> ActualizarItemEnCarritoAsync(string usuarioId, int productoId, int cantidad)
    {
        var cliente = await clienteOnlineRepository.GetByUsuarioIdentityIdAsync(usuarioId);
        if (cliente == null)
            throw new NotFoundException("Cliente no encontrado");
        if (cantidad <= 0)
            throw new BadRequestException("La cantidad debe ser mayor a 0");
        var producto = await productoRepository.GetByIdAsync(productoId);
        if (producto == null)
            throw new NotFoundException("Producto", productoId);
        if (!producto.TieneStock(cantidad))
            throw new BadRequestException($"Stock insuficiente para {producto.Nombre}. Disponible: {producto.Stock}, Solicitado: {cantidad}");
        var carrito = await pedidoRepository.GetCarritoByClienteIdAsync(cliente.Id);
        if (carrito == null)
            throw new BadRequestException("No tienes un carrito activo");
        var detalle = carrito.Detalles.FirstOrDefault(d => d.ProductoId == productoId);
        if (detalle == null)
            throw new NotFoundException("Producto no encontrado en el carrito");
        detalle.Cantidad = cantidad;
        carrito.CalcularTotal();
        await pedidoRepository.UpdateAsync(carrito);
        return MapToCarritoDto(carrito);
    }
    public async Task<bool> RemoverItemDelCarritoAsync(string usuarioId, int productoId)
    {
        var cliente = await clienteOnlineRepository.GetByUsuarioIdentityIdAsync(usuarioId);
        if (cliente == null)
            throw new NotFoundException("Cliente no encontrado");
        var carrito = await pedidoRepository.GetCarritoByClienteIdAsync(cliente.Id);
        if (carrito == null)
            throw new BadRequestException("No tienes un carrito activo");
        var detalle = carrito.Detalles.FirstOrDefault(d => d.ProductoId == productoId);
        if (detalle == null)
            throw new NotFoundException("Producto no encontrado en el carrito");
        carrito.Detalles.Remove(detalle);
        carrito.CalcularTotal();
        await pedidoRepository.UpdateAsync(carrito);
        return true;
    }
    public async Task<CarritoDto> LimpiarCarritoAsync(string usuarioId)
    {
        var cliente = await clienteOnlineRepository.GetByUsuarioIdentityIdAsync(usuarioId);
        if (cliente == null)
            throw new NotFoundException("Cliente no encontrado");
        var carrito = await pedidoRepository.GetCarritoByClienteIdAsync(cliente.Id);
        if (carrito == null)
            throw new BadRequestException("No tienes un carrito activo");
        carrito.Detalles.Clear();
        carrito.CalcularTotal();
        await pedidoRepository.UpdateAsync(carrito);
        return MapToCarritoDto(carrito);
    }
    public async Task<PedidoDto> ProcesarCheckoutDesdeCarritoAsync(string usuarioId, ProcesarCheckoutDto dto)
    {
        var cliente = await clienteOnlineRepository.GetByUsuarioIdentityIdAsync(usuarioId);
        if (cliente == null)
            throw new NotFoundException("Cliente no encontrado");
        var carrito = await pedidoRepository.GetCarritoByClienteIdAsync(cliente.Id);
        if (carrito == null || !carrito.Detalles.Any())
            throw new BadRequestException("El carrito está vacío");
        if (dto.FechaEntrega.Date < DateTime.UtcNow.Date)
            throw new BadRequestException("La fecha de entrega no puede ser anterior a hoy");
        if (dto.Observaciones?.Length > 500)
            throw new BadRequestException("Las observaciones no pueden exceder 500 caracteres");

        // Validar stock nuevamente
        foreach (var detalle in carrito.Detalles)
        {
            var producto = await productoRepository.GetByIdAsync(detalle.ProductoId);
            if (producto == null)
                throw new NotFoundException($"Producto con ID {detalle.ProductoId} no encontrado");
            if (!producto.Activo)
                throw new BadRequestException($"El producto {producto.Nombre} ya no está disponible");
            if (!producto.TieneStock(detalle.Cantidad))
                throw new BadRequestException($"Stock insuficiente para {producto.Nombre}. Disponible: {producto.Stock}, Solicitado: {detalle.Cantidad}");
        }
        // Aplicar descuento si es Efectivo o Transferencia
        if (dto.TipoPago.HasValue)
        {
            carrito.TipoPago = dto.TipoPago.Value;
            if (dto.TipoPago == TipoPagoEnum.Efectivo || dto.TipoPago == TipoPagoEnum.Transferencia)
            {
                var config = await configuracionRepository.GetByClaveAsync("DescuentoEfectivoTransferencia");
                var porcentaje = config != null ? decimal.Parse(config.Valor) : 10m;
                carrito.MontoConDescuento = carrito.Total * (100 - porcentaje) / 100;
            }
        }

        // Convertir carrito a pedido real (SIN descontar stock aún)
        carrito.Estado = EstadoPedidoEnum.Pendiente;
        carrito.FechaEntrega = DateTime.SpecifyKind(dto.FechaEntrega, DateTimeKind.Utc);
        carrito.Observaciones = dto.Observaciones;
        carrito.FechaPedido = DateTime.UtcNow;
        await pedidoRepository.UpdateAsync(carrito);
        var pedidoCompleto = await pedidoRepository.GetByIdWithDetallesAsync(carrito.Id);
        return MapToDto(pedidoCompleto!);
    }



    public async Task<PedidoDto> ProcesarPagoPedidoAsync(int pedidoId, string usuarioIdentity)
    {
        var cliente = await clienteOnlineRepository.GetByUsuarioIdentityIdAsync(usuarioIdentity);
        if (cliente == null)
            throw new NotFoundException("Cliente no encontrado");

        var pedido = await pedidoRepository.GetByIdWithDetallesAsync(pedidoId);
        if (pedido == null)
            throw new NotFoundException("Pedido", pedidoId);

        if (pedido.ClienteOnlineId != cliente.Id)
            throw new UnauthorizedException("No tiene permiso para procesar este pedido");

        if (pedido.Estado != EstadoPedidoEnum.Pendiente)
            throw new BadRequestException("El pedido no está en estado Pendiente");

        if (pedido.TipoPago == TipoPagoEnum.MercadoPago)
            throw new BadRequestException("Mercado Pago se procesa automáticamente vía webhook");

        if (pedido.EstadoPago == EstadoPagoEnum.Pagado)
            throw new BadRequestException("El pedido ya fue pagado");

        // Solo marca como Pagado. El admin confirma y descuenta stock
        // mediante el endpoint /api/admin/pedidos/{id}/confirmar-pago
        pedido.EstadoPago = EstadoPagoEnum.Pagado;
        pedido.FechaPago = DateTime.UtcNow;
        pedido.ReferenciaTransaccion = $"MANUAL-{pedido.Id}-{DateTime.UtcNow:yyyyMMddHHmmss}";

        await pedidoRepository.UpdateAsync(pedido);
        return MapToDto(pedido);
    }

    public async Task<DatosPagoPedidoDto> GetDatosPagoPedidoAsync(int pedidoId, string usuarioIdentity)
    {
        var cliente = await clienteOnlineRepository.GetByUsuarioIdentityIdAsync(usuarioIdentity);
        if (cliente == null)
            throw new NotFoundException("Cliente no encontrado");

        var pedido = await pedidoRepository.GetByIdWithDetallesAsync(pedidoId);
        if (pedido == null)
            throw new NotFoundException("Pedido", pedidoId);

        if (pedido.ClienteOnlineId != cliente.Id)
            throw new UnauthorizedException("No tiene permiso para ver este pedido");

        var dto = new DatosPagoPedidoDto
        {
            PedidoId = pedido.Id,
            Total = pedido.Total,
            MontoConDescuento = pedido.MontoConDescuento,
            TipoPago = pedido.TipoPago?.ToString(),
            EstadoPago = pedido.EstadoPago.ToString(),
            Estado = pedido.Estado.ToString()
        };

        var direccion = await direccionRetiroRepository.GetActivaAsync();
        if (direccion != null)
        {
            dto.DireccionRetiro = direccion.Direccion;
            dto.HorarioRetiro = $"{direccion.HorarioInicio} - {direccion.HorarioFin}";
            dto.TelefonoContacto = direccion.Telefono;
        }

        var datosBancarios = await datosBancariosRepository.GetActivoAsync();
        if (datosBancarios != null)
        {
            dto.DatosBancarios = datosBancarios.Adapt<DatosBancariosDto>();
        }

        return dto;
    }

    // ========== MAPPERS ==========
    private static PedidoDto MapToDto(Pedido pedido)
    {
        return new PedidoDto
        {
            Id = pedido.Id,
            ClienteOnlineId = pedido.ClienteOnlineId,
            ClienteNombre = pedido.ClienteOnline?.Nombre ?? string.Empty,
            ClienteEmail = pedido.ClienteOnline?.Email ?? string.Empty,
            ClienteTelefono = pedido.ClienteOnline?.Telefono,
            FechaPedido = pedido.FechaPedido,
            FechaEntrega = pedido.FechaEntrega,
            Estado = pedido.Estado.ToString(),
            EstadoId = (int)pedido.Estado,
            Total = pedido.Total,
            Observaciones = pedido.Observaciones,
            TipoPago = pedido.TipoPago?.ToString(),
            EstadoPago = pedido.EstadoPago.ToString(),
            MontoConDescuento = pedido.MontoConDescuento,
            ReferenciaTransaccion = pedido.ReferenciaTransaccion,
            FechaPago = pedido.FechaPago,
            MercadoPagoPreferenceId = pedido.MercadoPagoPreferenceId,
            MercadoPagoPaymentId = pedido.MercadoPagoPaymentId,
            PaymentStatus = pedido.PaymentStatus,
            Detalles = pedido.Detalles?.Select(d => new PedidoDetalleDto
            {
                Id = d.Id,
                ProductoId = d.ProductoId,
                ProductoNombre = d.Producto?.Nombre ?? string.Empty,
                ProductoImagen = d.Producto?.ImagenUrl
                    ?? d.Producto?.Imagenes?.FirstOrDefault(i => i.EsPrincipal)?.Url
                    ?? d.Producto?.Imagenes?.FirstOrDefault()?.Url,
                ProductoCategoria = d.Producto?.Categoria,
                Cantidad = d.Cantidad,
                PrecioUnitario = d.PrecioUnitario,
                Subtotal = d.Subtotal,
            }).ToList() ?? [],
        };
    }

    private static PedidoResumenDto MapToResumenDto(Pedido pedido)
    {
        return new PedidoResumenDto
        {
            Id = pedido.Id,
            ClienteNombre = pedido.ClienteOnline?.Nombre ?? string.Empty,
            FechaPedido = pedido.FechaPedido,
            FechaEntrega = pedido.FechaEntrega,
            Estado = pedido.Estado.ToString(),
            Total = pedido.Total,
            CantidadProductos = pedido.Detalles?.Sum(d => d.Cantidad) ?? 0,
            TipoPago = pedido.TipoPago?.ToString(),  
            EstadoPago = pedido.EstadoPago.ToString(),
            MontoConDescuento = pedido.MontoConDescuento,
            ClienteEmail = pedido.ClienteOnline?.Email ?? string.Empty,
            FechaPago = pedido.FechaPago,
            ReferenciaTransaccion = pedido.ReferenciaTransaccion,
            Detalles = pedido.Detalles?.Select(d => new PedidoDetalleDto
            {
                Id = d.Id,
                ProductoId = d.ProductoId,
                ProductoNombre = d.Producto?.Nombre ?? string.Empty,
                ProductoImagen = d.Producto?.ImagenUrl
                    ?? d.Producto?.Imagenes?.FirstOrDefault(i => i.EsPrincipal)?.Url
                    ?? d.Producto?.Imagenes?.FirstOrDefault()?.Url,
                ProductoCategoria = d.Producto?.Categoria,
                Cantidad = d.Cantidad,
                PrecioUnitario = d.PrecioUnitario,
                Subtotal = d.Subtotal
            }).ToList() ?? [],
        };
    }
    private static CarritoDto MapToCarritoDto(Pedido pedido)
    {
        return new CarritoDto
        {
            PedidoId = pedido.Id,
            Total = pedido.Total,
            TotalItems = pedido.Detalles?.Sum(d => d.Cantidad) ?? 0,
            Items = pedido.Detalles?.Select(d => new CarritoItemDto
            {
                ProductoId = d.ProductoId,
                ProductoNombre = d.Producto?.Nombre ?? string.Empty,
                ProductoImagen = d.Producto?.ImagenUrl
                    ?? d.Producto?.Imagenes?.FirstOrDefault(i => i.EsPrincipal)?.Url
                    ?? d.Producto?.Imagenes?.FirstOrDefault()?.Url,
                ProductoCategoria = d.Producto?.Categoria,
                PrecioUnitario = d.PrecioUnitario,
                Cantidad = d.Cantidad,
                Subtotal = d.Subtotal,
                EnOferta = d.Producto?.EnOferta ?? false,
                PrecioOferta = d.Producto?.PrecioOferta,
            }).ToList() ?? []
        };
    }
}
