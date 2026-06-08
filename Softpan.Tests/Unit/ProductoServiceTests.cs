using FluentAssertions;
using Moq;
using Softpan.Application.DTOs;
using Softpan.Application.Exceptions;
using Softpan.Application.Interfaces;
using Softpan.Application.Services;
using Softpan.Domain.Entities;
using Softpan.Domain.Interfaces;
using Xunit;

namespace Softpan.Tests.Unit.Services;

public class ProductoServiceTests
{
    private readonly Mock<IProductoRepository> _mockProductoRepository;
    private readonly Mock<IRedisCacheService> _mockCacheService;
    private readonly ProductoService _productoService;

    public ProductoServiceTests()
    {
        _mockProductoRepository = new Mock<IProductoRepository>();
        _mockCacheService = new Mock<IRedisCacheService>();
        _productoService = new ProductoService(_mockProductoRepository.Object, _mockCacheService.Object);
    }

    [Fact]
    public async Task GetProductoByIdAsync_ProductoExiste_RetornaProductoDto()
    {
        // Arrange
        var productoId = 1;
        var producto = new Producto
        {
            Id = productoId,
            Nombre = "Torta de Chocolate",
            PrecioBase = 5500,
            Stock = 10,
            Activo = true
        };

        _mockCacheService.Setup(x => x.GetAsync<ProductoDto>(It.IsAny<string>()))
            .ReturnsAsync((ProductoDto)null!);

        _mockProductoRepository.Setup(x => x.GetByIdAsync(productoId))
            .ReturnsAsync(producto);

        // Act
        var result = await _productoService.GetProductoByIdAsync(productoId);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(productoId);
        result.Nombre.Should().Be("Torta de Chocolate");
        result.PrecioBase.Should().Be(5500);
    }

    [Fact]
    public async Task GetProductoByIdAsync_ProductoNoExiste_LanzaNotFoundException()
    {
        // Arrange
        var productoId = 999;

        _mockCacheService.Setup(x => x.GetAsync<ProductoDto>(It.IsAny<string>()))
            .ReturnsAsync((ProductoDto)null!);

        _mockProductoRepository.Setup(x => x.GetByIdAsync(productoId))
            .ReturnsAsync((Producto)null!);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => 
            _productoService.GetProductoByIdAsync(productoId));
    }

    [Fact]
    public async Task CreateProductoAsync_DatosValidos_CreaProducto()
    {
        // Arrange
        var createDto = new CreateProductoDto
        {
            Nombre = "Nueva Torta",
            Descripcion = "Descripción",
            PrecioBase = 6000,
            Categoria = "Tortas",
            Stock = 5
        };

        var productoCreado = new Producto
        {
            Id = 1,
            Nombre = createDto.Nombre,
            PrecioBase = createDto.PrecioBase,
            Stock = createDto.Stock,
            Activo = true
        };

        _mockProductoRepository.Setup(x => x.CreateAsync(It.IsAny<Producto>()))
            .ReturnsAsync(productoCreado);

        // Act
        var result = await _productoService.CreateProductoAsync(createDto);

        // Assert
        result.Should().NotBeNull();
        result.Nombre.Should().Be(createDto.Nombre);
        result.PrecioBase.Should().Be(createDto.PrecioBase);
        _mockProductoRepository.Verify(x => x.CreateAsync(It.IsAny<Producto>()), Times.Once);
    }

    [Fact]
    public async Task GetProductosActivosAsync_RetornaListaDeProductosActivos()
    {
        // Arrange
        var productos = new List<Producto>
        {
            new() { Id = 1, Nombre = "Producto 1", Activo = true, PrecioBase = 100 },
            new() { Id = 2, Nombre = "Producto 2", Activo = true, PrecioBase = 200 }
        };

        _mockCacheService.Setup(x => x.GetAsync<IEnumerable<ProductoDto>>(It.IsAny<string>()))
            .ReturnsAsync((IEnumerable<ProductoDto>)null!);

        _mockProductoRepository.Setup(x => x.GetProductosActivosAsync())
            .ReturnsAsync(productos);

        // Act
        var result = await _productoService.GetProductosActivosAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task UpdateStockAsync_ProductoExiste_ActualizaStock()
    {
        // Arrange
        var productoId = 1;
        var nuevoStock = 20;
        var producto = new Producto
        {
            Id = productoId,
            Nombre = "Producto Test",
            Stock = 10,
            PrecioBase = 100
        };

        var updateStockDto = new UpdateStockDto { Stock = nuevoStock };

        _mockProductoRepository.Setup(x => x.GetByIdAsync(productoId))
            .ReturnsAsync(producto);

        _mockProductoRepository.Setup(x => x.UpdateAsync(It.IsAny<Producto>()))
            .ReturnsAsync(producto);

        // Act
        var result = await _productoService.UpdateStockAsync(productoId, updateStockDto);

        // Assert
        result.Should().NotBeNull();
        _mockProductoRepository.Verify(x => x.UpdateAsync(It.Is<Producto>(p => p.Stock == nuevoStock)), Times.Once);
    }

    [Fact]
    public async Task BuscarProductosAsync_ConQuery_RetornaProductosFiltrados()
    {
        // Arrange
        var query = "chocolate";
        var productos = new List<Producto>
        {
            new() { Id = 1, Nombre = "Torta de Chocolate", Activo = true, PrecioBase = 100 },
            new() { Id = 2, Nombre = "Brownie de Chocolate", Activo = true, PrecioBase = 50 },
            new() { Id = 3, Nombre = "Torta de Frutilla", Activo = true, PrecioBase = 120 }
        };

        _mockCacheService.Setup(x => x.GetAsync<IEnumerable<ProductoDto>>(It.IsAny<string>()))
            .ReturnsAsync((IEnumerable<ProductoDto>)null!);

        _mockProductoRepository.Setup(x => x.BuscarProductosAsync(query))
            .ReturnsAsync(() => productos.Where(p =>
                p.Nombre.Contains(query, StringComparison.OrdinalIgnoreCase)).ToList());

        // Act
        var result = await _productoService.BuscarProductosAsync(query);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.All(p => p.Nombre.ToLower().Contains(query)).Should().BeTrue();
    }
}
