
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
             .AsNoTracking()
             .Include(p => p.PreciosPersonalizados)
             .ThenInclude(pc => pc.Cliente)
             .Include(p => p.Imagenes)
             .FirstOrDefaultAsync(p => p.Id == id);
    }
    public async Task<IEnumerable<Producto>> GetAllAsync()
    {
        return await context.Productos
            .AsNoTracking()
            .Include(p => p.Imagenes)  // ← AGREGAR ESTO
            .ToListAsync();
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
            .AsNoTracking()
            .Where(p => p.Activo)
            .Include(p => p.Imagenes)  // ← AGREGAR ESTO
            .ToListAsync();
    }

    public async Task<decimal> GetPrecioClienteAsync(int productoId, int clienteId)
    {
        var precioPersonalizado = await context.PrecioClientes
            .AsNoTracking()
            .FirstOrDefaultAsync(pc => pc.ProductoId == productoId && pc.ClienteId == clienteId);

        if (precioPersonalizado != null)
            return precioPersonalizado.Precio;

        var producto = await context.Productos.FindAsync(productoId);
        return producto?.PrecioBase ?? 0;
    }

    public async Task<IEnumerable<Producto>> GetProductosInmediatoAsync()
    {
        return await context.Productos
            .AsNoTracking()
            .Include(p => p.Imagenes)
            .Where(p => p.Activo && p.StockInmediato)
            .ToListAsync();
    }

    public async Task<IEnumerable<Producto>> GetProductosEnOfertaAsync()
    {
        return await context.Productos
            .AsNoTracking()
            .Include(p => p.Imagenes)
            .Where(p => p.Activo && p.EnOferta && p.PrecioOferta != null)
            .ToListAsync();
    }

    public async Task<IEnumerable<Producto>> BuscarProductosAsync(string query)
    {
        var queryLower = query.ToLower();

        return await context.Productos
            .AsNoTracking()
            .Include(p => p.Imagenes)
            .Where(p => p.Activo &&
                (p.Nombre.ToLower().Contains(queryLower) ||
                 (p.Descripcion != null && p.Descripcion.ToLower().Contains(queryLower)) ||
                 (p.Categoria != null && p.Categoria.ToLower().Contains(queryLower))))
            .ToListAsync();
    }

}
