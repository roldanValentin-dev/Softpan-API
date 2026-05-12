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
    private readonly Mock<IPedidoRepository> _mockPedidoRepo;
    private readonly Mock<IClienteOnlineRepository> _mockClienteRepo;
    private readonly Mock<IProductoRepository> _mockProductoRepo;
    private readonly Mock<IConfiguracionRepository> _mockConfigRepo;
    private readonly Mock<IDatosBancariosRepository> _mockDatosBancariosRepo;
    private readonly Mock<IDireccionRetiroRepository> _mockDireccionRepo;
    private readonly PedidoService _service;

    public PedidoServiceTests()
    {
        _mockPedidoRepo = new Mock<IPedidoRepository>();
        _mockClienteRepo = new Mock<IClienteOnlineRepository>();
        _mockProductoRepo = new Mock<IProductoRepository>();
        _mockConfigRepo = new Mock<IConfiguracionRepository>();
        _mockDatosBancariosRepo = new Mock<IDatosBancariosRepository>();
        _mockDireccionRepo = new Mock<IDireccionRetiroRepository>();
        _service = new PedidoService(
            _mockPedidoRepo.Object,
            _mockClienteRepo.Object,
            _mockProductoRepo.Object,
            _mockConfigRepo.Object,
            _mockDatosBancariosRepo.Object,
            _mockDireccionRepo.Object
        );
    }

    // ========================================================================
    // HELPERS
    // ========================================================================
    private ClienteOnline CrearCliente(string usuarioId = "user123", int id = 1)
    {
        return new ClienteOnline
        {
            Id = id,
            Nombre = "Juan",
            Email = "juan@email.com",
            UsuarioIdentityId = usuarioId
        };
    }

    private Producto CrearProducto(int id = 1, string nombre = "Torta", decimal precio = 5000, int stock = 10)
    {
        return new Producto
        {
            Id = id,
            Nombre = nombre,
            PrecioBase = precio,
            Stock = stock,
            Activo = true
        };
    }

    private Pedido CrearPedido(ClienteOnline cliente, EstadoPedidoEnum estado = EstadoPedidoEnum.Pendiente,
        List<PedidoDetalle>? detalles = null)
    {
        return new Pedido
        {
            Id = 1,
            ClienteOnlineId = cliente.Id,
            ClienteOnline = cliente,
            Estado = estado,
            Total = 10000,
            Detalles = detalles ?? new List<PedidoDetalle>
            {
                new()
                {
                    Id = 1,
                    ProductoId = 1,
                    Producto = CrearProducto(),
                    Cantidad = 2,
                    PrecioUnitario = 5000
                }
            }
        };
    }

    // ========================================================================
    // CREATE PEDIDO
    // ========================================================================
    [Fact]
    public async Task CreatePedidoAsync_DatosValidos_CreaPedido()
    {
        var usuarioId = "user123";
        var cliente = CrearCliente();
        var producto = CrearProducto();
        var pedidoCreado = CrearPedido(cliente);

        _mockClienteRepo.Setup(x => x.GetByUsuarioIdentityIdAsync(usuarioId)).ReturnsAsync(cliente);
        _mockProductoRepo.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(producto);
        _mockPedidoRepo.Setup(x => x.CreateAsync(It.IsAny<Pedido>())).ReturnsAsync(pedidoCreado);
        _mockPedidoRepo.Setup(x => x.GetByIdWithDetallesAsync(It.IsAny<int>())).ReturnsAsync(pedidoCreado);

        var dto = new CreatePedidoDto
        {
            FechaEntrega = DateTime.Today.AddDays(2),
            Observaciones = "Test",
            Detalles = new List<CreatePedidoDetalleDto> { new() { ProductoId = 1, Cantidad = 2 } }
        };

        var result = await _service.CreatePedidoAsync(usuarioId, dto);

        result.Should().NotBeNull();
        result.Total.Should().Be(10000);
        result.Estado.Should().Be("Pendiente");
    }

    [Fact]
    public async Task CreatePedidoAsync_ConTipoPagoEfectivo_AplicaDescuento()
    {
        var usuarioId = "user123";
        var cliente = CrearCliente();
        var producto = CrearProducto(precio: 1000, stock: 10);
        Pedido? pedidoGuardado = null;

        _mockClienteRepo.Setup(x => x.GetByUsuarioIdentityIdAsync(usuarioId)).ReturnsAsync(cliente);
        _mockProductoRepo.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(producto);
        _mockPedidoRepo.Setup(x => x.CreateAsync(It.IsAny<Pedido>()))
            .Callback<Pedido>(p => pedidoGuardado = p)
            .ReturnsAsync(() => pedidoGuardado!);
        _mockPedidoRepo.Setup(x => x.GetByIdWithDetallesAsync(It.IsAny<int>()))
            .ReturnsAsync(() => pedidoGuardado!);

        var dto = new CreatePedidoDto
        {
            FechaEntrega = DateTime.Today.AddDays(2),
            TipoPago = TipoPagoEnum.Efectivo,
            Detalles = new List<CreatePedidoDetalleDto> { new() { ProductoId = 1, Cantidad = 2 } }
        };

        var result = await _service.CreatePedidoAsync(usuarioId, dto);

        result.MontoConDescuento.Should().Be(1800); // 2000 * 0.9
        result.TipoPago.Should().Be("Efectivo");
    }

    [Fact]
    public async Task CreatePedidoAsync_ConTipoPagoMercadoPago_NoAplicaDescuento()
    {
        var usuarioId = "user123";
        var cliente = CrearCliente();
        var producto = CrearProducto(precio: 1000, stock: 10);
        Pedido? pedidoGuardado = null;

        _mockClienteRepo.Setup(x => x.GetByUsuarioIdentityIdAsync(usuarioId)).ReturnsAsync(cliente);
        _mockProductoRepo.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(producto);
        _mockPedidoRepo.Setup(x => x.CreateAsync(It.IsAny<Pedido>()))
            .Callback<Pedido>(p => pedidoGuardado = p)
            .ReturnsAsync(() => pedidoGuardado!);
        _mockPedidoRepo.Setup(x => x.GetByIdWithDetallesAsync(It.IsAny<int>()))
            .ReturnsAsync(() => pedidoGuardado!);

        var dto = new CreatePedidoDto
        {
            FechaEntrega = DateTime.Today.AddDays(2),
            TipoPago = TipoPagoEnum.MercadoPago,
            Detalles = new List<CreatePedidoDetalleDto> { new() { ProductoId = 1, Cantidad = 2 } }
        };

        var result = await _service.CreatePedidoAsync(usuarioId, dto);

        result.MontoConDescuento.Should().BeNull();
        result.TipoPago.Should().Be("MercadoPago");
    }

    [Fact]
    public async Task CreatePedidoAsync_ClienteNoExiste_LanzaNotFound()
    {
        _mockClienteRepo.Setup(x => x.GetByUsuarioIdentityIdAsync("user999"))
            .ReturnsAsync((ClienteOnline)null!);

        var dto = new CreatePedidoDto
        {
            FechaEntrega = DateTime.Today.AddDays(2),
            Detalles = new List<CreatePedidoDetalleDto> { new() { ProductoId = 1, Cantidad = 2 } }
        };

        await Assert.ThrowsAsync<NotFoundException>(() =>
            _service.CreatePedidoAsync("user999", dto));
    }

    [Fact]
    public async Task CreatePedidoAsync_StockInsuficiente_LanzaBadRequest()
    {
        var usuarioId = "user123";
        _mockClienteRepo.Setup(x => x.GetByUsuarioIdentityIdAsync(usuarioId))
            .ReturnsAsync(CrearCliente());
        _mockProductoRepo.Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(CrearProducto(stock: 1));

        var dto = new CreatePedidoDto
        {
            FechaEntrega = DateTime.Today.AddDays(2),
            Detalles = new List<CreatePedidoDetalleDto> { new() { ProductoId = 1, Cantidad = 5 } }
        };

        var ex = await Assert.ThrowsAsync<BadRequestException>(() =>
            _service.CreatePedidoAsync(usuarioId, dto));
        ex.Message.Should().Contain("Stock insuficiente");
    }

    // ========================================================================
    // CANCELAR
    // ========================================================================
    [Fact]
    public async Task CancelarPedidoAsync_PedidoPendiente_Cancela()
    {
        var usuarioId = "user123";
        var cliente = CrearCliente();
        var pedido = CrearPedido(cliente, EstadoPedidoEnum.Pendiente);

        _mockClienteRepo.Setup(x => x.GetByUsuarioIdentityIdAsync(usuarioId)).ReturnsAsync(cliente);
        _mockPedidoRepo.Setup(x => x.GetByIdWithDetallesAsync(1)).ReturnsAsync(pedido);
        _mockPedidoRepo.Setup(x => x.UpdateAsync(It.IsAny<Pedido>())).ReturnsAsync(pedido);

        var result = await _service.CancelarPedidoAsync(1, usuarioId);

        result.Estado.Should().Be("Cancelado");
    }

    [Fact]
    public async Task CancelarPedidoAsync_PedidoConfirmado_LanzaBadRequest()
    {
        var usuarioId = "user123";
        var cliente = CrearCliente();
        var pedido = CrearPedido(cliente, EstadoPedidoEnum.Confirmado);

        _mockClienteRepo.Setup(x => x.GetByUsuarioIdentityIdAsync(usuarioId)).ReturnsAsync(cliente);
        _mockPedidoRepo.Setup(x => x.GetByIdWithDetallesAsync(1)).ReturnsAsync(pedido);

        await Assert.ThrowsAsync<BadRequestException>(() =>
            _service.CancelarPedidoAsync(1, usuarioId));
    }

    // ========================================================================
    // CARRITO
    // ========================================================================
    [Fact]
    public async Task ObtenerOCrearCarritoAsync_SinCarrito_CreaNuevo()
    {
        var usuarioId = "user123";
        var cliente = CrearCliente();
        Pedido? carritoCreado = null;

        _mockClienteRepo.Setup(x => x.GetByUsuarioIdentityIdAsync(usuarioId)).ReturnsAsync(cliente);
        _mockPedidoRepo.Setup(x => x.GetCarritoByClienteIdAsync(cliente.Id)).ReturnsAsync((Pedido?)null);
        _mockPedidoRepo.Setup(x => x.CreateAsync(It.IsAny<Pedido>()))
            .Callback<Pedido>(p => carritoCreado = p)
            .ReturnsAsync(() => carritoCreado!);

        var result = await _service.ObtenerOCrearCarritoAsync(usuarioId);

        result.Should().NotBeNull();
        result.Items.Should().BeEmpty();
        result.Total.Should().Be(0);
        _mockPedidoRepo.Verify(x => x.CreateAsync(It.Is<Pedido>(p => p.Estado == EstadoPedidoEnum.Carrito)), Times.Once);
    }

    [Fact]
    public async Task ObtenerOCrearCarritoAsync_CarritoExistente_RetornaExistente()
    {
        var usuarioId = "user123";
        var cliente = CrearCliente();
        var carrito = CrearPedido(cliente, EstadoPedidoEnum.Carrito);

        _mockClienteRepo.Setup(x => x.GetByUsuarioIdentityIdAsync(usuarioId)).ReturnsAsync(cliente);
        _mockPedidoRepo.Setup(x => x.GetCarritoByClienteIdAsync(cliente.Id)).ReturnsAsync(carrito);

        var result = await _service.ObtenerOCrearCarritoAsync(usuarioId);

        result.PedidoId.Should().Be(1);
        _mockPedidoRepo.Verify(x => x.CreateAsync(It.IsAny<Pedido>()), Times.Never);
    }

    [Fact]
    public async Task AgregarItemAlCarritoAsync_NuevoItem_AgregaCorrectamente()
    {
        var usuarioId = "user123";
        var cliente = CrearCliente();
        var producto = CrearProducto();
        var carrito = new Pedido
        {
            Id = 1,
            ClienteOnlineId = cliente.Id,
            Estado = EstadoPedidoEnum.Carrito,
            Detalles = new List<PedidoDetalle>()
        };

        _mockClienteRepo.Setup(x => x.GetByUsuarioIdentityIdAsync(usuarioId)).ReturnsAsync(cliente);
        _mockProductoRepo.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(producto);
        _mockPedidoRepo.Setup(x => x.GetCarritoByClienteIdAsync(cliente.Id)).ReturnsAsync(carrito);
        _mockPedidoRepo.Setup(x => x.UpdateAsync(It.IsAny<Pedido>())).ReturnsAsync(carrito);

        var result = await _service.AgregarItemAlCarritoAsync(usuarioId, 1, 3);

        result.Items.Should().HaveCount(1);
        result.Items[0].Cantidad.Should().Be(3);
        result.Items[0].PrecioUnitario.Should().Be(5000);
    }

    [Fact]
    public async Task AgregarItemAlCarritoAsync_ItemExistente_SumaCantidades()
    {
        var usuarioId = "user123";
        var cliente = CrearCliente();
        var producto = CrearProducto(stock: 20);
        var carrito = new Pedido
        {
            Id = 1,
            ClienteOnlineId = cliente.Id,
            Estado = EstadoPedidoEnum.Carrito,
            Detalles = new List<PedidoDetalle>
            {
                new() { ProductoId = 1, Cantidad = 2, PrecioUnitario = 5000, Producto = producto }
            }
        };

        _mockClienteRepo.Setup(x => x.GetByUsuarioIdentityIdAsync(usuarioId)).ReturnsAsync(cliente);
        _mockProductoRepo.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(producto);
        _mockPedidoRepo.Setup(x => x.GetCarritoByClienteIdAsync(cliente.Id)).ReturnsAsync(carrito);
        _mockPedidoRepo.Setup(x => x.UpdateAsync(It.IsAny<Pedido>())).ReturnsAsync(carrito);

        var result = await _service.AgregarItemAlCarritoAsync(usuarioId, 1, 3);

        result.Items[0].Cantidad.Should().Be(5); // 2 + 3
    }

    [Fact]
    public async Task AgregarItemAlCarritoAsync_StockInsuficiente_LanzaBadRequest()
    {
        var usuarioId = "user123";
        _mockClienteRepo.Setup(x => x.GetByUsuarioIdentityIdAsync(usuarioId)).ReturnsAsync(CrearCliente());
        _mockProductoRepo.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(CrearProducto(stock: 2));

        await Assert.ThrowsAsync<BadRequestException>(() =>
            _service.AgregarItemAlCarritoAsync(usuarioId, 1, 5));
    }

    [Fact]
    public async Task ActualizarItemEnCarritoAsync_ItemExistente_ActualizaCorrectamente()
    {
        var usuarioId = "user123";
        var cliente = CrearCliente();
        var producto = CrearProducto(stock: 20);
        var carrito = new Pedido
        {
            Id = 1,
            ClienteOnlineId = cliente.Id,
            Estado = EstadoPedidoEnum.Carrito,
            Detalles = new List<PedidoDetalle>
            {
                new() { ProductoId = 1, Cantidad = 2, PrecioUnitario = 5000, Producto = producto }
            }
        };

        _mockClienteRepo.Setup(x => x.GetByUsuarioIdentityIdAsync(usuarioId)).ReturnsAsync(cliente);
        _mockProductoRepo.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(producto);
        _mockPedidoRepo.Setup(x => x.GetCarritoByClienteIdAsync(cliente.Id)).ReturnsAsync(carrito);
        _mockPedidoRepo.Setup(x => x.UpdateAsync(It.IsAny<Pedido>())).ReturnsAsync(carrito);

        var result = await _service.ActualizarItemEnCarritoAsync(usuarioId, 1, 10);

        result.Items[0].Cantidad.Should().Be(10);
    }

    [Fact]
    public async Task RemoverItemDelCarritoAsync_ItemExistente_Remueve()
    {
        var usuarioId = "user123";
        var cliente = CrearCliente();
        var producto = CrearProducto();
        var carrito = new Pedido
        {
            Id = 1,
            ClienteOnlineId = cliente.Id,
            Estado = EstadoPedidoEnum.Carrito,
            Detalles = new List<PedidoDetalle>
            {
                new() { ProductoId = 1, Cantidad = 2, PrecioUnitario = 5000, Producto = producto }
            }
        };

        _mockClienteRepo.Setup(x => x.GetByUsuarioIdentityIdAsync(usuarioId)).ReturnsAsync(cliente);
        _mockPedidoRepo.Setup(x => x.GetCarritoByClienteIdAsync(cliente.Id)).ReturnsAsync(carrito);
        _mockPedidoRepo.Setup(x => x.UpdateAsync(It.IsAny<Pedido>())).ReturnsAsync(carrito);

        var result = await _service.RemoverItemDelCarritoAsync(usuarioId, 1);

        result.Should().BeTrue();
        carrito.Detalles.Should().BeEmpty();
    }

    [Fact]
    public async Task LimpiarCarritoAsync_CarritoExistente_LimpiaItems()
    {
        var usuarioId = "user123";
        var cliente = CrearCliente();
        var producto = CrearProducto();
        var carrito = new Pedido
        {
            Id = 1,
            ClienteOnlineId = cliente.Id,
            Estado = EstadoPedidoEnum.Carrito,
            Detalles = new List<PedidoDetalle>
            {
                new() { ProductoId = 1, Cantidad = 2, Producto = producto }
            }
        };

        _mockClienteRepo.Setup(x => x.GetByUsuarioIdentityIdAsync(usuarioId)).ReturnsAsync(cliente);
        _mockPedidoRepo.Setup(x => x.GetCarritoByClienteIdAsync(cliente.Id)).ReturnsAsync(carrito);
        _mockPedidoRepo.Setup(x => x.UpdateAsync(It.IsAny<Pedido>())).ReturnsAsync(carrito);

        var result = await _service.LimpiarCarritoAsync(usuarioId);

        result.Items.Should().BeEmpty();
        result.Total.Should().Be(0);
    }

    [Fact]
    public async Task ProcesarCheckoutDesdeCarritoAsync_CarritoValido_CreaPedido()
    {
        var usuarioId = "user123";
        var cliente = CrearCliente();
        var producto = CrearProducto(stock: 10);
        var carrito = new Pedido
        {
            Id = 1,
            ClienteOnlineId = cliente.Id,
            Estado = EstadoPedidoEnum.Carrito,
            Detalles = new List<PedidoDetalle>
            {
                new() { ProductoId = 1, Cantidad = 2, PrecioUnitario = 5000, Producto = producto }
            }
        };

        _mockClienteRepo.Setup(x => x.GetByUsuarioIdentityIdAsync(usuarioId)).ReturnsAsync(cliente);
        _mockProductoRepo.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(producto);
        _mockPedidoRepo.Setup(x => x.GetCarritoByClienteIdAsync(cliente.Id)).ReturnsAsync(carrito);
        _mockPedidoRepo.Setup(x => x.UpdateAsync(It.IsAny<Pedido>())).ReturnsAsync(carrito);
        _mockPedidoRepo.Setup(x => x.GetByIdWithDetallesAsync(1)).ReturnsAsync(carrito);

        var dto = new ProcesarCheckoutDto
        {
            FechaEntrega = DateTime.Today.AddDays(2),
            Observaciones = "Test"
        };

        var result = await _service.ProcesarCheckoutDesdeCarritoAsync(usuarioId, dto);

        result.Should().NotBeNull();
        result.Estado.Should().Be("Pendiente");
    }

    [Fact]
    public async Task ProcesarCheckoutDesdeCarritoAsync_CarritoVacio_LanzaBadRequest()
    {
        var usuarioId = "user123";
        var cliente = CrearCliente();
        var carritoVacio = new Pedido
        {
            Id = 1,
            ClienteOnlineId = cliente.Id,
            Estado = EstadoPedidoEnum.Carrito,
            Detalles = new List<PedidoDetalle>()
        };

        _mockClienteRepo.Setup(x => x.GetByUsuarioIdentityIdAsync(usuarioId)).ReturnsAsync(cliente);
        _mockPedidoRepo.Setup(x => x.GetCarritoByClienteIdAsync(cliente.Id)).ReturnsAsync(carritoVacio);

        var dto = new ProcesarCheckoutDto { FechaEntrega = DateTime.Today.AddDays(2) };

        await Assert.ThrowsAsync<BadRequestException>(() =>
            _service.ProcesarCheckoutDesdeCarritoAsync(usuarioId, dto));
    }

    // ========================================================================
    // PROCESAR PAGO
    // ========================================================================
    [Fact]
    public async Task ProcesarPagoPedidoAsync_PagoValido_MarcaPagado()
    {
        var usuarioId = "user123";
        var cliente = CrearCliente();
        var producto = CrearProducto(stock: 10);
        var pedido = new Pedido
        {
            Id = 1,
            ClienteOnlineId = cliente.Id,
            ClienteOnline = cliente,
            Estado = EstadoPedidoEnum.Pendiente,
            TipoPago = TipoPagoEnum.Efectivo,
            EstadoPago = EstadoPagoEnum.Pendiente,
            Detalles = new List<PedidoDetalle>
            {
                new() { ProductoId = 1, Cantidad = 2, Producto = producto }
            }
        };

        _mockClienteRepo.Setup(x => x.GetByUsuarioIdentityIdAsync(usuarioId)).ReturnsAsync(cliente);
        _mockPedidoRepo.Setup(x => x.GetByIdWithDetallesAsync(1)).ReturnsAsync(pedido);
        _mockPedidoRepo.Setup(x => x.UpdateAsync(It.IsAny<Pedido>())).ReturnsAsync(pedido);

        var result = await _service.ProcesarPagoPedidoAsync(1, usuarioId);

        result.EstadoPago.Should().Be("Pagado");
        result.Estado.Should().Be("Pendiente"); // Sigue Pendiente, admin confirma
        producto.Stock.Should().Be(10); // Stock NO se descuenta aún
    }

    [Fact]
    public async Task ProcesarPagoPedidoAsync_PedidoMercadoPago_LanzaBadRequest()
    {
        var usuarioId = "user123";
        var cliente = CrearCliente();
        var pedido = new Pedido
        {
            Id = 1,
            ClienteOnlineId = cliente.Id,
            ClienteOnline = cliente,
            Estado = EstadoPedidoEnum.Pendiente,
            TipoPago = TipoPagoEnum.MercadoPago,
            Detalles = new List<PedidoDetalle>()
        };

        _mockClienteRepo.Setup(x => x.GetByUsuarioIdentityIdAsync(usuarioId)).ReturnsAsync(cliente);
        _mockPedidoRepo.Setup(x => x.GetByIdWithDetallesAsync(1)).ReturnsAsync(pedido);

        await Assert.ThrowsAsync<BadRequestException>(() =>
            _service.ProcesarPagoPedidoAsync(1, usuarioId));
    }

    [Fact]
    public async Task ProcesarPagoPedidoAsync_YaPagado_LanzaBadRequest()
    {
        var usuarioId = "user123";
        var cliente = CrearCliente();
        var pedido = new Pedido
        {
            Id = 1,
            ClienteOnlineId = cliente.Id,
            ClienteOnline = cliente,
            Estado = EstadoPedidoEnum.Pendiente,
            TipoPago = TipoPagoEnum.Efectivo,
            EstadoPago = EstadoPagoEnum.Pagado,
            Detalles = new List<PedidoDetalle>()
        };

        _mockClienteRepo.Setup(x => x.GetByUsuarioIdentityIdAsync(usuarioId)).ReturnsAsync(cliente);
        _mockPedidoRepo.Setup(x => x.GetByIdWithDetallesAsync(1)).ReturnsAsync(pedido);

        await Assert.ThrowsAsync<BadRequestException>(() =>
            _service.ProcesarPagoPedidoAsync(1, usuarioId));
    }

    // ========================================================================
    // UPDATE ESTADO - GUARDA DE PAGO
    // ========================================================================
    [Fact]
    public async Task UpdateEstadoPedidoAsync_SinPago_NoPermiteConfirmar()
    {
        var cliente = CrearCliente();
        var pedido = CrearPedido(cliente, EstadoPedidoEnum.Pendiente);
        pedido.EstadoPago = EstadoPagoEnum.Pendiente; // No pagado

        _mockPedidoRepo.Setup(x => x.GetByIdWithDetallesAsync(1)).ReturnsAsync(pedido);

        var dto = new UpdateEstadoPedidoDto { EstadoId = 2 }; // Confirmado

        await Assert.ThrowsAsync<BadRequestException>(() =>
            _service.UpdateEstadoPedidoAsync(1, dto));
    }

    [Fact]
    public async Task UpdateEstadoPedidoAsync_Pagado_PermiteConfirmar()
    {
        var cliente = CrearCliente();
        var producto = CrearProducto(stock: 10);
        var pedido = CrearPedido(cliente, EstadoPedidoEnum.Pendiente);
        pedido.EstadoPago = EstadoPagoEnum.Pagado;
        pedido.Detalles = new List<PedidoDetalle>
        {
            new() { ProductoId = 1, Cantidad = 2, Producto = producto }
        };

        _mockPedidoRepo.Setup(x => x.GetByIdWithDetallesAsync(1)).ReturnsAsync(pedido);
        _mockProductoRepo.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(producto);
        _mockPedidoRepo.Setup(x => x.UpdateAsync(It.IsAny<Pedido>())).ReturnsAsync(pedido);

        var dto = new UpdateEstadoPedidoDto { EstadoId = 2 };

        var result = await _service.UpdateEstadoPedidoAsync(1, dto);

        result.Estado.Should().Be("Confirmado");
        producto.Stock.Should().Be(8); // Stock descontado
    }

    [Fact]
    public async Task UpdateEstadoPedidoAsync_Carrito_LanzaBadRequest()
    {
        var pedido = new Pedido { Id = 1, Estado = EstadoPedidoEnum.Carrito };
        _mockPedidoRepo.Setup(x => x.GetByIdWithDetallesAsync(1)).ReturnsAsync(pedido);

        var dto = new UpdateEstadoPedidoDto { EstadoId = 2 };

        await Assert.ThrowsAsync<BadRequestException>(() =>
            _service.UpdateEstadoPedidoAsync(1, dto));
    }
}
