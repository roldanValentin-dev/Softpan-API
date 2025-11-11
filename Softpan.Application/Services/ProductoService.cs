using Mapster;
using Softpan.Application.DTOs;
using Softpan.Application.Interfaces;
using Softpan.Domain.Entities;
using Softpan.Domain.Interfaces;

namespace Softpan.Application.Services;

public class ProductoService(IProductoRepository productoRepository) : IProductoService
{
    public async Task<ProductoDto?> GetProductoByIdAsync(int id)
    {
        var producto = await productoRepository.GetByIdAsync(id);
        if (producto == null)
        {
            return null;
        }
        return MapToDto(producto);
    }

    public async Task<ProductoDetalleDto?> GetProductoDetalleByIdAsync(int id)
    {
        var producto = await productoRepository.GetByIdAsync(id);
        if (producto == null)
        {
            return null;
        }
        return producto.Adapt<ProductoDetalleDto>();
    }

    public async Task<IEnumerable<ProductoDto>> GetAllProductosAsync()
    {
        var productos = await productoRepository.GetAllAsync();
        return productos.Select(MapToDto);
    }

    public async Task<IEnumerable<ProductoDto>> GetProductosActivosAsync()
    {
        var productos = await productoRepository.GetProductosActivosAsync();
        return productos.Select(MapToDto);
    }

    public async Task<ProductoDto> CreateProductoAsync(CreateProductoDto dto)
    {
        var producto = dto.Adapt<Producto>();

        var createdProducto = await productoRepository.CreateAsync(producto);

        return MapToDto(createdProducto);
    }

    public async Task<ProductoDto> UpdateProductoAsync(UpdateProductoDto dto)
    {
        var producto = dto.Adapt<Producto>();

        var updatedProducto = await productoRepository.UpdateAsync(producto);

        return MapToDto(updatedProducto!);
    }

    public async Task<bool> DeleteProductoAsync(int id)
    {
        return await productoRepository.DeleteAsync(id);
    }

    private static ProductoDto MapToDto(Producto producto) => producto.Adapt<ProductoDto>();
}