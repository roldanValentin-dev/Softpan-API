using Mapster;
using Softpan.Application.DTOs;
using Softpan.Application.Exceptions;
using Softpan.Application.Interfaces;
using Softpan.Domain.Entities;
using Softpan.Domain.Interfaces;

namespace Softpan.Application.Services;

public class ProductoImagenService : IProductoImagenService
{
    private readonly IProductoImagenRepository _imagenRepository;
    private readonly IProductoRepository _productoRepository;
    private readonly IRedisCacheService _cacheService;
    private readonly IFileStorageService _fileStorageService;

    public ProductoImagenService(
        IProductoImagenRepository imagenRepository,
        IProductoRepository productoRepository,
        IRedisCacheService cacheService,
        IFileStorageService fileStorageService)
    {
        _imagenRepository = imagenRepository;
        _productoRepository = productoRepository;
        _cacheService = cacheService;
        _fileStorageService = fileStorageService;
    }

    public async Task<List<ProductoImagenDto>> GetImagenesByProductoIdAsync(int productoId)
    {
        var producto = await _productoRepository.GetByIdAsync(productoId);
        if (producto == null)
            throw new NotFoundException("Producto", productoId);

        var imagenes = await _imagenRepository.GetByProductoIdAsync(productoId);
        return imagenes.Select(i => i.Adapt<ProductoImagenDto>()).ToList();
    }

    public async Task<ProductoImagenDto> CreateImagenAsync(int productoId, CreateProductoImagenDto dto)
    {
        var producto = await _productoRepository.GetByIdAsync(productoId);
        if (producto == null)
            throw new NotFoundException("Producto", productoId);

        // Si se marca como principal, desmarcar las demás
        if (dto.EsPrincipal)
        {
            await _imagenRepository.DesmarcarPrincipalAsync(productoId);
        }

        var imagen = new ProductoImagen
        {
            ProductoId = productoId,
            Url = dto.Url,
            Orden = dto.Orden,
            EsPrincipal = dto.EsPrincipal,
            FechaCreacion = DateTime.UtcNow
        };

        var imagenCreada = await _imagenRepository.CreateAsync(imagen);

        // Invalidar caché del producto
        await InvalidarCacheProducto(productoId);

        return imagenCreada.Adapt<ProductoImagenDto>();
    }

    public async Task<ProductoImagenDto> UpdateImagenAsync(int id, UpdateProductoImagenDto dto)
    {
        var imagen = await _imagenRepository.GetByIdAsync(id);
        if (imagen == null)
            throw new NotFoundException("Imagen", id);

        // Si se marca como principal, desmarcar las demás
        if (dto.EsPrincipal && !imagen.EsPrincipal)
        {
            await _imagenRepository.DesmarcarPrincipalAsync(imagen.ProductoId);
        }

        imagen.Orden = dto.Orden;
        imagen.EsPrincipal = dto.EsPrincipal;

        var imagenActualizada = await _imagenRepository.UpdateAsync(imagen);

        // Invalidar caché del producto
        await InvalidarCacheProducto(imagen.ProductoId);

        return imagenActualizada.Adapt<ProductoImagenDto>();
    }

    public async Task<bool> DeleteImagenAsync(int id)
    {
        var imagen = await _imagenRepository.GetByIdAsync(id);
        if (imagen == null)
            throw new NotFoundException("Imagen", id);

        var productoId = imagen.ProductoId;
        var imageUrl = imagen.Url;
        
        // 1. Eliminar registro de la base de datos
        var result = await _imagenRepository.DeleteAsync(id);

        if (result)
        {
            // 2. Eliminar archivo físico del servidor
            await _fileStorageService.DeleteFileAsync(imageUrl);
            
            // 3. Invalidar caché del producto
            await InvalidarCacheProducto(productoId);
        }

        return result;
    }

    private async Task InvalidarCacheProducto(int productoId)
    {
        await _cacheService.RemoveAsync($"producto:{productoId}");
        await _cacheService.RemoveAsync($"producto:{productoId}:detalle");
        await _cacheService.RemoveAsync("productos:todos");
        await _cacheService.RemoveAsync("productos:activos");
    }
}
