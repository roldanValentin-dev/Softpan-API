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
        return pedidos.Select(MapToDto).ToList();
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

    // ========== MÉTODOS PARA ADMIN ==========

    public async Task<List<PedidoResumenDto>> GetAllPedidosAsync()
    {
        var pedidos = await pedidoRepository.GetAllAsync();
        return pedidos.Select(MapToResumenDto).ToList();
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

        pedido.Estado = (EstadoPedidoEnum)dto.EstadoId;

        var pedidoActualizado = await pedidoRepository.UpdateAsync(pedido);
        return MapToDto(pedidoActualizado);
    }

    // ========== MAPPERS ==========

    private static PedidoDto MapToDto(Pedido pedido)
    {
        var dto = pedido.Adapt<PedidoDto>();
        dto.Estado = pedido.Estado.ToString();
        dto.EstadoId = (int)pedido.Estado;
        dto.ClienteNombre = pedido.ClienteOnline?.Nombre ?? string.Empty;
        dto.ClienteEmail = pedido.ClienteOnline?.Email ?? string.Empty;
        dto.ClienteTelefono = pedido.ClienteOnline?.Telefono;
        
        if (pedido.Detalles != null && pedido.Detalles.Any())
        {
            dto.Detalles = pedido.Detalles.Select(d => new PedidoDetalleDto
            {
                Id = d.Id,
                ProductoId = d.ProductoId,
                ProductoNombre = d.Producto?.Nombre ?? string.Empty,
                ProductoImagen = d.Producto?.ImagenUrl,
                ProductoCategoria = d.Producto?.Categoria,
                Cantidad = d.Cantidad,
                PrecioUnitario = d.PrecioUnitario,
                Subtotal = d.Subtotal
            }).ToList();
        }

        return dto;
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
            CantidadProductos = pedido.Detalles?.Sum(d => d.Cantidad) ?? 0
        };
    }
}
