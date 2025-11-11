
using Microsoft.EntityFrameworkCore;
using Softpan.Domain.Entities;
using Softpan.Domain.Interfaces;
using Softpan.Infrastructure.Data;

namespace Softpan.Infrastructure.Repositories;

public class ClienteRepository(ApplicationDbContext context) : IClienteRepository
{

    public async Task<IEnumerable<Cliente>> GetAllAsync()
    {
        return await context.Clientes.ToListAsync();
    }

    public async Task<Cliente?> GetByIdAsync(int id)
    {
        return await context.Clientes.FindAsync(id);
    }

    public async Task<Cliente> CreateAsync(Cliente cliente)
    {
        context.Clientes.Add(cliente);
        await context.SaveChangesAsync();
        return cliente;
    }

    public async Task<Cliente> UpdateAsync(Cliente cliente)
    {
        context.Clientes.Update(cliente);
        await context.SaveChangesAsync();
        return cliente;
    }
    public async Task<bool> DeleteAsync(int id)
    {
        var cliente = await context.Clientes.FindAsync(id);
        if (cliente == null) return false;

        cliente.Activo = false;  
        return await context.SaveChangesAsync() > 0;
        
    }
    public async Task<IEnumerable<Cliente>> GetClientsWithDebts()
    {
        return await context.Clientes
            .Include(c => c.Ventas)
            .Where(c => c.Activo && c.Ventas.Any(v => v.MontoTotal - v.MontoPagado > 0))
            .ToListAsync();
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await context.Clientes.AnyAsync(c => c.Id == id);
    }

}
