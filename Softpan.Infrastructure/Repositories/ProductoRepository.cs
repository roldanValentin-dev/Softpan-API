
using Microsoft.EntityFrameworkCore;
using Softpan.Domain.Entities;
using Softpan.Domain.Interfaces;
using Softpan.Infrastructure.Data;

namespace Softpan.Infrastructure.Repositories;

public class ProductoRepository(ApplicationDbContext context) : IProductoRepository
{

    public async Task<Producto?> GetByIdAsync(int id)
    {
        return await context.Productos
             .Include(p => p.PreciosPersonalizados)
             .ThenInclude(pc => pc.Cliente)
             .FirstOrDefaultAsync(p => p.Id == id);
    }
    public async Task<IEnumerable<Producto>> GetAllAsync()
    {
        return await context.Productos.ToListAsync();
    }

    public async Task<Producto> CreateAsync(Producto producto)
    {
        context.Add(producto);
        await context.SaveChangesAsync();
        return producto;
    }

    public async Task<Producto?> UpdateAsync(Producto producto)
    {
        context.Productos.Update(producto);
        await context.SaveChangesAsync();
        return producto;
    }
    public async Task<bool> DeleteAsync(int id)
    {
        var producto = await context.Productos.FindAsync(id);

        if (producto == null) return false;

        producto.Activo = false;
        return await context.SaveChangesAsync() > 0;
    }
    public async Task<bool> ExistsAsync(int id)
    {
        return await context.Productos.AnyAsync(p => p.Id == id);
    }

    public async Task<IEnumerable<Producto>> GetProductosActivosAsync()
    {
        return await context.Productos
            .Where(p => p.Activo)
            .ToListAsync();
    }

    public async Task<decimal> GetPrecioClienteAsync(int productoId, int clienteId)
    {
        var precioPersonalizado = await context.PrecioClientes
            .FirstOrDefaultAsync(pc => pc.ProductoId == productoId && pc.ClienteId == clienteId);

        if (precioPersonalizado != null)
            return precioPersonalizado.Precio;

        var producto = await context.Productos.FindAsync(productoId);
        return producto?.PrecioBase ?? 0;
    }

}
