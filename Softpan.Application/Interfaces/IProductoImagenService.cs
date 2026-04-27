using Softpan.Application.DTOs;

namespace Softpan.Application.Interfaces;

public interface IProductoImagenService
{
    Task<List<ProductoImagenDto>> GetImagenesByProductoIdAsync(int productoId);
    Task<ProductoImagenDto> CreateImagenAsync(int productoId, CreateProductoImagenDto dto);
    Task<ProductoImagenDto> UpdateImagenAsync(int id, UpdateProductoImagenDto dto);
    Task<bool> DeleteImagenAsync(int id);
}
