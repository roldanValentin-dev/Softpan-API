using Microsoft.EntityFrameworkCore;
using Softpan.Domain.Entities;
using Softpan.Domain.Interfaces;
using Softpan.Infrastructure.Data;

namespace Softpan.Infrastructure.Repositories;

public class ProductoImagenRepository : IProductoImagenRepository
{
    private readonly ApplicationDbContext _context;

    public ProductoImagenRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ProductoImagen?> GetByIdAsync(int id)
    {
        return await _context.Set<ProductoImagen>()
            .AsNoTracking()
            .FirstOrDefaultAsync(pi => pi.Id == id);
    }

    public async Task<List<ProductoImagen>> GetByProductoIdAsync(int productoId)
    {
        return await _context.Set<ProductoImagen>()
            .AsNoTracking()
            .Where(pi => pi.ProductoId == productoId)
            .OrderBy(pi => pi.Orden)
            .ToListAsync();
    }

    public async Task<ProductoImagen> CreateAsync(ProductoImagen imagen)
    {
        _context.Set<ProductoImagen>().Add(imagen);
        await _context.SaveChangesAsync();
        return imagen;
    }

    public async Task<ProductoImagen> UpdateAsync(ProductoImagen imagen)
    {
        _context.Set<ProductoImagen>().Update(imagen);
        await _context.SaveChangesAsync();
        return imagen;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var imagen = await _context.Set<ProductoImagen>().FindAsync(id);
        if (imagen == null) return false;

        _context.Set<ProductoImagen>().Remove(imagen);
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.Set<ProductoImagen>().AnyAsync(pi => pi.Id == id);
    }

    public async Task DesmarcarPrincipalAsync(int productoId)
    {
        var imagenes = await _context.Set<ProductoImagen>()
            .Where(pi => pi.ProductoId == productoId && pi.EsPrincipal)
            .ToListAsync();

        foreach (var imagen in imagenes)
        {
            imagen.EsPrincipal = false;
        }

        await _context.SaveChangesAsync();
    }
}
