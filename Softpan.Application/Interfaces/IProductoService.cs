using Softpan.Application.DTOs;


namespace Softpan.Application.Interfaces;

public interface IProductoService
{
    Task<ProductoDto?> GetProductoByIdAsync(int id);
    Task<ProductoDetalleDto?> GetProductoDetalleByIdAsync(int id);
    Task<IEnumerable<ProductoDto>> GetAllProductosAsync();
    Task<IEnumerable<ProductoDto>> GetProductosActivosAsync();
    Task<ProductoDto> CreateProductoAsync(CreateProductoDto dto);

    Task<IEnumerable<ProductoDto>> GetProductosByCategoriaAsync(string categoria);
    Task<IEnumerable<string>> GetCategoriasAsync();
    Task<IEnumerable<ProductoDto>> BuscarProductosAsync(string query);
    Task<ProductoDto> UpdateProductoAsync(int id,UpdateProductoDto dto);
    Task<ProductoDto> UpdateStockAsync(int id, UpdateStockDto dto);
    Task<bool> DeleteProductoAsync(int id);
    Task<IEnumerable<ProductoDto>> GetProductosInmediatoAsync();
    Task<IEnumerable<ProductoDto>> GetProductosEnOfertaAsync();
}