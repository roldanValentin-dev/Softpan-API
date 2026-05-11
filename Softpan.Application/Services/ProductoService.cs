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
        await cacheService.RemoveAsync("productos:categorias");

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
        await cacheService.RemoveAsync("productos:categorias");

        return MapToDto(updatedProducto!);
    }

    public async Task<IEnumerable<ProductoDto>> GetProductosByCategoriaAsync(string categoria)
    {
        var cacheKey = $"productos:categoria:{categoria}";
        var cacheProductos = await cacheService.GetAsync<IEnumerable<ProductoDto>>(cacheKey);
        
        if (cacheProductos != null)
        {
            return cacheProductos;
        }

        var productos = await productoRepository.GetProductosActivosAsync();
        var productosFiltrados = productos
            .Where(p => p.Categoria != null && p.Categoria.Equals(categoria, StringComparison.OrdinalIgnoreCase))
            .ToList();

        var dto = productosFiltrados.Select(MapToDto).ToList();
        await cacheService.SetAsync(cacheKey, dto, TimeSpan.FromMinutes(10));
        
        return dto;
    }

    public async Task<IEnumerable<string>> GetCategoriasAsync()
    {
        var cacheKey = "productos:categorias";
        var cacheCategorias = await cacheService.GetAsync<IEnumerable<string>>(cacheKey);
        
        if (cacheCategorias != null)
        {
            return cacheCategorias;
        }

        var productos = await productoRepository.GetAllAsync();
        var categorias = productos
            .Where(p => !string.IsNullOrEmpty(p.Categoria))
            .Select(p => p.Categoria!)
            .Distinct()
            .OrderBy(c => c)
            .ToList();

        await cacheService.SetAsync(cacheKey, categorias, TimeSpan.FromMinutes(30));
        
        return categorias;
    }

    public async Task<IEnumerable<ProductoDto>> BuscarProductosAsync(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return await GetProductosActivosAsync();
        }

        var cacheKey = $"productos:busqueda:{query.ToLower()}";
        var cacheProductos = await cacheService.GetAsync<IEnumerable<ProductoDto>>(cacheKey);
        
        if (cacheProductos != null)
        {
            return cacheProductos;
        }

        // OPTIMIZACIÓN: Búsqueda en base de datos en lugar de en memoria
        var productos = await productoRepository.BuscarProductosAsync(query);
        var dto = productos.Select(MapToDto).ToList();
        await cacheService.SetAsync(cacheKey, dto, TimeSpan.FromMinutes(5));
        
        return dto;
    }

    public async Task<ProductoDto> UpdateStockAsync(int id, UpdateStockDto dto)
    {
        var producto = await productoRepository.GetByIdAsync(id);
        if (producto == null)
        {
            throw new NotFoundException("Producto", id);
        }

        producto.Stock = dto.Stock;
        producto.FechaModificacion = DateTime.UtcNow;

        var updatedProducto = await productoRepository.UpdateAsync(producto);

        await cacheService.RemoveAsync($"producto:{id}");
        await cacheService.RemoveAsync($"producto:{id}:detalle");
        await cacheService.RemoveAsync("productos:todos");
        await cacheService.RemoveAsync("productos:activos");

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
            await cacheService.RemoveAsync("productos:categorias");
        }

        return result;
    }

    private static ProductoDto MapToDto(Producto producto) => producto.Adapt<ProductoDto>();
}