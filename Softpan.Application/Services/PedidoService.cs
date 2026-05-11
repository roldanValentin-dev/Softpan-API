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
    IProductoRepository productoRepository) : IPedidoService
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
            FechaEntrega = dto.FechaEntrega,
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
        if (pedido.StockDescontado)
        {
            foreach (var detalle in pedido.Detalles)
            {
                var producto = await productoRepository.GetByIdAsync(detalle.ProductoId);
                if (producto != null)
                {
                    producto.RestaurarStock(detalle.Cantidad);
                    await productoRepository.UpdateAsync(producto);
                }
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

        // Si se confirma el pedido, descontar stock
        if (estadoAnterior == EstadoPedidoEnum.Pendiente && estadoNuevo == EstadoPedidoEnum.Confirmado)
        {
            foreach (var detalle in pedido.Detalles)
            {
                var producto = await productoRepository.GetByIdAsync(detalle.ProductoId);
                if (producto != null)
                {
                    producto.DescontarStock(detalle.Cantidad);
                    await productoRepository.UpdateAsync(producto);
                }
            }
            pedido.StockDescontado = true;
        }

        // Si se cancela desde admin, restaurar stock si fue descontado
        if (estadoNuevo == EstadoPedidoEnum.Cancelado && pedido.StockDescontado)
        {
            foreach (var detalle in pedido.Detalles)
            {
                var producto = await productoRepository.GetByIdAsync(detalle.ProductoId);
                if (producto != null)
                {
                    producto.RestaurarStock(detalle.Cantidad);
                    await productoRepository.UpdateAsync(producto);
                }
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
        return MapToCarritoDto(carrito);
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
        // Convertir carrito a pedido real (SIN descontar stock aún)
        carrito.Estado = EstadoPedidoEnum.Pendiente;
        carrito.FechaEntrega = dto.FechaEntrega;
        carrito.Observaciones = dto.Observaciones;
        carrito.FechaPedido = DateTime.UtcNow;
        await pedidoRepository.UpdateAsync(carrito);
        var pedidoCompleto = await pedidoRepository.GetByIdWithDetallesAsync(carrito.Id);
        return MapToDto(pedidoCompleto!);
    }



    // ========== MAPPERS ==========
    private static PedidoDto MapToDto(Pedido pedido) => pedido.Adapt<PedidoDto>();

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
            CantidadProductos = pedido.Detalles?.Sum(d => d.Cantidad) ?? 0
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
                ProductoImagen = d.Producto?.ImagenUrl,
                ProductoCategoria = d.Producto?.Categoria,
                PrecioUnitario = d.PrecioUnitario,
                Cantidad = d.Cantidad,
                Subtotal = d.Subtotal
            }).ToList() ?? []
        };
    }
}
