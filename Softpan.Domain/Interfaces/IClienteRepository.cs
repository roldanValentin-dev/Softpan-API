
using Softpan.Domain.Entities;

namespace Softpan.Domain.Interfaces;

public interface IClienteRepository
{
    Task<Cliente?> GetByIdAsync(int id);
    Task<IEnumerable<Cliente>> GetAllAsync();

    Task<Cliente> CreateAsync(Cliente cliente);
    Task<Cliente> UpdateAsync(Cliente cliente);
    Task<bool> DeleteAsync(int id);

    Task<IEnumerable<Cliente>> GetClientsWithDebts();
    Task<bool> ExistsAsync(int id);

}

