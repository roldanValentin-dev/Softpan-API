using Mapster;
using Softpan.Application.DTOs;
using Softpan.Application.Exceptions;
using Softpan.Application.Interfaces;
using Softpan.Domain.Entities;
using Softpan.Domain.Interfaces;

namespace Softpan.Application.Services;

public class ProductoService(IProductoRepository productoRepository, IRedisCacheService cacheService) : IProductoService
{
    public async Task<ProductoDto?> GetProductoByIdAsync(int id)
    {
        var cacheProducto = await cacheService.GetAsync<ProductoDto>($"producto:{id}");
        if (cacheProducto != null)
        {
            //retornamos cache
            return cacheProducto;
        }


        var producto = await productoRepository.GetByIdAsync(id);
        if (producto == null)
        {
            throw new NotFoundException("Producto", id);
        }
        var dto = MapToDto(producto);

        await cacheService.SetAsync($"producto:{id}", dto, TimeSpan.FromMinutes(10));

        return dto;

    }

    public async Task<ProductoDetalleDto?> GetProductoDetalleByIdAsync(int id)
    {
        var cacheProducto = await cacheService.GetAsync<ProductoDetalleDto>($"producto:{id}:detalle");

        if (cacheProducto != null)
        {
            return cacheProducto;
        }

        var producto = await productoRepository.GetByIdAsync(id);
        if (producto == null)
        {
            throw new NotFoundException("Producto", id);
        }
        var dto = producto.Adapt<ProductoDetalleDto>();
        await cacheService.SetAsync($"producto:{id}:detalle", dto, TimeSpan.FromMinutes(15));

        return dto;
    }

    public async Task<IEnumerable<ProductoDto>> GetAllProductosAsync()
    {
        var cacheProductos = await cacheService.GetAsync<IEnumerable<ProductoDto>>("productos:todos");
        if (cacheProductos != null)
        {
            return cacheProductos;
        }

        var productos = await productoRepository.GetAllAsync();
        var dto = productos.Select(MapToDto).ToList();
        await cacheService.SetAsync("productos:todos", dto, TimeSpan.FromMinutes(10));
        return dto;
    }

    public async Task<IEnumerable<ProductoDto>> GetProductosActivosAsync()
    {
        var cacheProductos = await cacheService.GetAsync<IEnumerable<ProductoDto>>("productos:activos");
        if (cacheProductos != null)
        {
            return cacheProductos;
        }
        var productos = await productoRepository.GetProductosActivosAsync();
        var dto = productos.Select(MapToDto).ToList();
        await cacheService.SetAsync($"productos:activos", dto, TimeSpan.FromMinutes(10));
        return dto;
    }

    public async Task<ProductoDto> CreateProductoAsync(CreateProductoDto dto)
    {
        var producto = dto.Adapt<Producto>();

        var createdProducto = await productoRepository.CreateAsync(producto);
        await cacheService.RemoveAsync("productos:todos");
        await cacheService.RemoveAsync("productos:activos");

        return MapToDto(createdProducto);
    }

    public async Task<ProductoDto> UpdateProductoAsync(int id, UpdateProductoDto dto)
    {
        if (id != dto.Id)
        {
            throw new BadRequestException("El ID de la URL no coincide con el ID del body");
        }

        var existingProducto = await productoRepository.GetByIdAsync(id);
        if (existingProducto == null)
        {
            throw new NotFoundException("Producto", id);
        }

        dto.Adapt(existingProducto);

        var updatedProducto = await productoRepository.UpdateAsync(existingProducto);

        await cacheService.RemoveAsync("productos:todos");
        await cacheService.RemoveAsync("productos:activos");
        await cacheService.RemoveAsync($"producto:{id}");
        await cacheService.RemoveAsync($"producto:{id}:detalle");

        return MapToDto(updatedProducto!);
    }

    public async Task<bool> DeleteProductoAsync(int id)
    {
        var result = await productoRepository.DeleteAsync(id);

        if (result)
        {
            await cacheService.RemoveAsync($"producto:{id}");
            await cacheService.RemoveAsync($"producto:{id}:detalle");
            await cacheService.RemoveAsync("productos:todos");
            await cacheService.RemoveAsync("productos:activos");
        }

        return result;
    }

    private static ProductoDto MapToDto(Producto producto) => producto.Adapt<ProductoDto>();
}