

using Microsoft.EntityFrameworkCore;
using Softpan.Domain.Entities;
using Softpan.Domain.Interfaces;
using Softpan.Infrastructure.Data;

namespace Softpan.Infrastructure.Repositories;

public class ClientesOnlineRepository : IClienteOnlineRepository
{
    private readonly ApplicationDbContext _context;

    public ClientesOnlineRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ClienteOnline?> GetByIdAsync (int id)
    {
        return await _context.ClientesOnline
            .Include(c => c.Usuario)
            .FirstOrDefaultAsync(c => c.Id == id);
    }
    public async Task<ClienteOnline?> GetByUsuarioIdentityIdAsync(string usuarioIdentityId)
    {
        return await _context.ClientesOnline
            .Include(c => c.Usuario)
            .FirstOrDefaultAsync(c => c.UsuarioIdentityId == usuarioIdentityId);
    }
    public async Task<ClienteOnline?> GetByEmailAsync(string email)
    {
        return await _context.ClientesOnline
            .Include(c => c.Usuario)
            .FirstOrDefaultAsync(c => c.Email == email);
    }
    public async Task<ClienteOnline> CreateAsync(ClienteOnline cliente)
    {
        _context.ClientesOnline.Add(cliente);
        await _context.SaveChangesAsync();
        return cliente;
    }

    public async Task<ClienteOnline> UpdateAsync(ClienteOnline cliente)
    {
        _context.ClientesOnline.Update(cliente);
        await _context.SaveChangesAsync();
        return cliente;
    }

    public async Task<bool> ExistsByEmailAsync(string email)
    {
        return await _context.ClientesOnline
            .AnyAsync(c => c.Email == email);
    }
}
