using Softpan.Application.DTOs;


namespace Softpan.Application.Interfaces;

public interface IProductoService
{
    Task<ProductoDto?> GetProductoByIdAsync(int id);
    Task<ProductoDetalleDto?> GetProductoDetalleByIdAsync(int id);
    Task<IEnumerable<ProductoDto>> GetAllProductosAsync();
    Task<IEnumerable<ProductoDto>> GetProductosActivosAsync();
    Task<ProductoDto> CreateProductoAsync(CreateProductoDto dto);
    Task<ProductoDto> UpdateProductoAsync(int id,UpdateProductoDto dto);
    Task<bool> DeleteProductoAsync(int id);
}