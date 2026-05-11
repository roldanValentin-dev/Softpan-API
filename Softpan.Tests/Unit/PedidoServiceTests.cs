using FluentAssertions;
using Moq;
using Softpan.Application.DTOs;
using Softpan.Application.Exceptions;
using Softpan.Application.Services;
using Softpan.Domain.Entities;
using Softpan.Domain.Enums;
using Softpan.Domain.Interfaces;
using Xunit;

namespace Softpan.Tests.Unit.Services;

public class PedidoServiceTests
{
    private readonly Mock<IPedidoRepository> _mockPedidoRepository;
    private readonly Mock<IClienteOnlineRepository> _mockClienteOnlineRepository;
    private readonly Mock<IProductoRepository> _mockProductoRepository;
    private readonly PedidoService _pedidoService;

    public PedidoServiceTests()
    {
        _mockPedidoRepository = new Mock<IPedidoRepository>();
        _mockClienteOnlineRepository = new Mock<IClienteOnlineRepository>();
        _mockProductoRepository = new Mock<IProductoRepository>();
        _pedidoService = new PedidoService(
            _mockPedidoRepository.Object,
            _mockClienteOnlineRepository.Object,
            _mockProductoRepository.Object
        );
    }

    [Fact]
    public async Task CreatePedidoAsync_DatosValidos_CreaPedido()
    {
        // Arrange
        var usuarioId = "user123";
        var cliente = new ClienteOnline
        {
            Id = 1,
            Nombre = "Juan",
            Email = "juan@email.com",
            UsuarioIdentityId = usuarioId
        };

        var producto = new Producto
        {
            Id = 1,
            Nombre = "Torta",
            PrecioBase = 5000,
            Stock = 10,
            Activo = true
        };

        var createDto = new CreatePedidoDto
        {
            FechaEntrega = DateTime.Today.AddDays(2),
            Observaciones = "Test",
            Detalles = new List<CreatePedidoDetalleDto>
            {
                new() { ProductoId = 1, Cantidad = 2 }
            }
        };

        var pedidoCreado = new Pedido
        {
            Id = 1,
            ClienteOnlineId = cliente.Id,
            ClienteOnline = cliente,
            FechaPedido = DateTime.UtcNow,
            FechaEntrega = createDto.FechaEntrega,
            Estado = EstadoPedidoEnum.Pendiente,
            Total = 10000,
            Detalles = new List<PedidoDetalle>
            {
                new()
                {
                    Id = 1,
                    ProductoId = 1,
                    Producto = producto,
                    Cantidad = 2,
                    PrecioUnitario = 5000
                }
            }
        };

        _mockClienteOnlineRepository.Setup(x => x.GetByUsuarioIdentityIdAsync(usuarioId))
            .ReturnsAsync(cliente);

        _mockProductoRepository.Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(producto);

        _mockPedidoRepository.Setup(x => x.CreateAsync(It.IsAny<Pedido>()))
            .ReturnsAsync(pedidoCreado);

        _mockPedidoRepository.Setup(x => x.GetByIdWithDetallesAsync(It.IsAny<int>()))
            .ReturnsAsync(pedidoCreado);

        // Act
        var result = await _pedidoService.CreatePedidoAsync(usuarioId, createDto);

        // Assert
        result.Should().NotBeNull();
        result.Total.Should().Be(10000);
        result.Estado.Should().Be("Pendiente");
        result.Detalles.Should().HaveCount(1);
    }

    [Fact]
    public async Task CreatePedidoAsync_ClienteNoExiste_LanzaNotFoundException()
    {
        // Arrange
        var usuarioId = "user999";
        var createDto = new CreatePedidoDto
        {
            FechaEntrega = DateTime.Today.AddDays(2),
            Detalles = new List<CreatePedidoDetalleDto>
            {
                new() { ProductoId = 1, Cantidad = 2 }
            }
        };

        _mockClienteOnlineRepository.Setup(x => x.GetByUsuarioIdentityIdAsync(usuarioId))
            .ReturnsAsync((ClienteOnline)null!);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _pedidoService.CreatePedidoAsync(usuarioId, createDto));
    }

    [Fact]
    public async Task CreatePedidoAsync_SinDetalles_LanzaBadRequestException()
    {
        // Arrange
        var usuarioId = "user123";
        var cliente = new ClienteOnline { Id = 1, UsuarioIdentityId = usuarioId };

        var createDto = new CreatePedidoDto
        {
            FechaEntrega = DateTime.Today.AddDays(2),
            Detalles = new List<CreatePedidoDetalleDto>()
        };

        _mockClienteOnlineRepository.Setup(x => x.GetByUsuarioIdentityIdAsync(usuarioId))
            .ReturnsAsync(cliente);

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() =>
            _pedidoService.CreatePedidoAsync(usuarioId, createDto));
    }

    [Fact]
    public async Task CreatePedidoAsync_StockInsuficiente_LanzaBadRequestException()
    {
        // Arrange
        var usuarioId = "user123";
        var cliente = new ClienteOnline { Id = 1, UsuarioIdentityId = usuarioId };
        var producto = new Producto
        {
            Id = 1,
            Nombre = "Torta",
            Stock = 1,
            Activo = true,
            PrecioBase = 5000
        };

        var createDto = new CreatePedidoDto
        {
            FechaEntrega = DateTime.Today.AddDays(2),
            Detalles = new List<CreatePedidoDetalleDto>
            {
                new() { ProductoId = 1, Cantidad = 5 } // Más que el stock disponible
            }
        };

        _mockClienteOnlineRepository.Setup(x => x.GetByUsuarioIdentityIdAsync(usuarioId))
            .ReturnsAsync(cliente);

        _mockProductoRepository.Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(producto);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BadRequestException>(() =>
            _pedidoService.CreatePedidoAsync(usuarioId, createDto));

        exception.Message.Should().Contain("Stock insuficiente");
    }

    [Fact]
    public async Task CancelarPedidoAsync_PedidoPendiente_CancelaPedido()
    {
        // Arrange
        var usuarioId = "user123";
        var cliente = new ClienteOnline { Id = 1, UsuarioIdentityId = usuarioId };
        var pedido = new Pedido
        {
            Id = 1,
            ClienteOnlineId = 1,
            ClienteOnline = cliente,
            Estado = EstadoPedidoEnum.Pendiente,
            StockDescontado = false,
            Detalles = new List<PedidoDetalle>()
        };

        _mockClienteOnlineRepository.Setup(x => x.GetByUsuarioIdentityIdAsync(usuarioId))
            .ReturnsAsync(cliente);

        _mockPedidoRepository.Setup(x => x.GetByIdWithDetallesAsync(1))
            .ReturnsAsync(pedido);

        _mockPedidoRepository.Setup(x => x.UpdateAsync(It.IsAny<Pedido>()))
            .ReturnsAsync(pedido);

        // Act
        var result = await _pedidoService.CancelarPedidoAsync(1, usuarioId);

        // Assert
        result.Should().NotBeNull();
        result.Estado.Should().Be("Cancelado");
        _mockPedidoRepository.Verify(x => x.UpdateAsync(It.Is<Pedido>(p => 
            p.Estado == EstadoPedidoEnum.Cancelado)), Times.Once);
    }

    [Fact]
    public async Task CancelarPedidoAsync_PedidoConfirmado_LanzaBadRequestException()
    {
        // Arrange
        var usuarioId = "user123";
        var cliente = new ClienteOnline { Id = 1, UsuarioIdentityId = usuarioId };
        var pedido = new Pedido
        {
            Id = 1,
            ClienteOnlineId = 1,
            ClienteOnline = cliente,
            Estado = EstadoPedidoEnum.Confirmado,
            Detalles = new List<PedidoDetalle>()
        };

        _mockClienteOnlineRepository.Setup(x => x.GetByUsuarioIdentityIdAsync(usuarioId))
            .ReturnsAsync(cliente);

        _mockPedidoRepository.Setup(x => x.GetByIdWithDetallesAsync(1))
            .ReturnsAsync(pedido);

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() =>
            _pedidoService.CancelarPedidoAsync(1, usuarioId));
    }
}
