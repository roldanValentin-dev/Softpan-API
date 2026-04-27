using Softpan.Domain.Entities;

namespace Softpan.Domain.Interfaces;

public interface IProductoImagenRepository
{
    Task<ProductoImagen?> GetByIdAsync(int id);
    Task<List<ProductoImagen>> GetByProductoIdAsync(int productoId);
    Task<ProductoImagen> CreateAsync(ProductoImagen imagen);
    Task<ProductoImagen> UpdateAsync(ProductoImagen imagen);
    Task<bool> DeleteAsync(int id);
    Task<bool> ExistsAsync(int id);
    Task DesmarcarPrincipalAsync(int productoId);
}
