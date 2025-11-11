
using Mapster;
using Softpan.Application.DTOs;
using Softpan.Application.Interfaces;
using Softpan.Domain.Entities;
using Softpan.Domain.Interfaces;

namespace Softpan.Application.Services;

public class ClienteService(IClienteRepository clienteRepository) : IClienteService
{

    public async Task<ClienteDto?> GetClientByIdAsync(int id)
    {
        var cliente = await clienteRepository.GetByIdAsync(id);
        if (cliente == null)
        {
            return null;
        }
        return MapToDto(cliente);
    }
    public async Task<IEnumerable<ClienteDto>> GetAllClientsAsync()
    {
        var clientes = await clienteRepository.GetAllAsync();
        return clientes.Select(MapToDto);
    }

    public async Task<ClienteDto> CreateClientAsync(CreateClienteDto clienteDto)
    {
        var cliente = clienteDto.Adapt<Cliente>();
        var createCliente = await clienteRepository.CreateAsync(cliente);

        return MapToDto(createCliente);
        
    }
    public async Task<ClienteDto> UpdateClientAsync(UpdateClienteDto clienteDto)
    {
        var cliente = clienteDto.Adapt<Cliente>();
        var updateCliente = await clienteRepository.UpdateAsync(cliente);
        return MapToDto(updateCliente);
    }

    public async Task<bool> DeleteClientAsync(int id)
    {
        var cliente = await clienteRepository.DeleteAsync(id);
        return cliente;
    }
    public async Task<IEnumerable<ClienteDto>> GetClientsWithDebtsAsync()
    {
        var clientes = await clienteRepository.GetClientsWithDebts();
        return clientes.Select(MapToDto);
    }


    private static ClienteDto MapToDto(Cliente cliente) => cliente.Adapt<ClienteDto>();
}
