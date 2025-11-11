
using Softpan.Domain.Entities;

namespace Softpan.Domain.Interfaces;

public interface IProductoRepository
{
    public Task<Producto?> GetByIdAsync(int id);

    public Task<IEnumerable<Producto>> GetAllAsync();

    public Task<Producto> CreateAsync (Producto producto);

    public Task<Producto?> UpdateAsync (Producto producto);

    public Task<bool> DeleteAsync (int id);

    public Task<bool> ExistsAsync (int id);


    public Task<IEnumerable<Producto>> GetProductosActivosAsync();

    public Task<decimal> GetPrecioClienteAsync(int productoId, int clienteId);




}
