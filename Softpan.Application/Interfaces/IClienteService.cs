
using Softpan.Application.DTOs;

namespace Softpan.Application.Interfaces;

public interface IClienteService
{
    Task<ClienteDto?> GetClientByIdAsync(int id);
    Task<IEnumerable<ClienteDto>> GetAllClientsAsync();
    Task<ClienteDto> CreateClientAsync(CreateClienteDto clienteDto);
    Task<ClienteDto> UpdateClientAsync(int id,UpdateClienteDto clienteDto);
    Task<bool> DeleteClientAsync(int id);
    Task<IEnumerable<ClienteDto>> GetClientsWithDebtsAsync();
}
