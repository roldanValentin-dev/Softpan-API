
using Softpan.Domain.Entities;

namespace Softpan.Domain.Interfaces;

public interface IProductoRepository
{
    Task<Producto?> GetByIdAsync(int id);

    Task<IEnumerable<Producto>> GetAllAsync();

    Task<Producto> CreateAsync(Producto producto);

    Task<Producto?> UpdateAsync(Producto producto);

    Task<bool> DeleteAsync(int id);

    Task<bool> ExistsAsync(int id);

    Task<IEnumerable<Producto>> GetProductosActivosAsync();

    Task<decimal> GetPrecioClienteAsync(int productoId, int clienteId);

    Task<IEnumerable<Producto>> BuscarProductosAsync(string query);
    Task<IEnumerable<Producto>> GetProductosInmediatoAsync();
    Task<IEnumerable<Producto>> GetProductosEnOfertaAsync();
}
