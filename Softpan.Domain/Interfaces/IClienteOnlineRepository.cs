

using Softpan.Domain.Entities;

namespace Softpan.Domain.Interfaces;

public interface IClienteOnlineRepository
{
    Task<ClienteOnline?> GetByIdAsync(int id);
    Task<ClienteOnline?> GetByUsuarioIdentityIdAsync(string usuarioIdentityId);
    Task<ClienteOnline?> GetByEmailAsync(string email);
    Task<ClienteOnline> CreateAsync(ClienteOnline cliente);
    Task<ClienteOnline> UpdateAsync(ClienteOnline cliente);
    Task<bool> ExistsByEmailAsync(string email);
}
