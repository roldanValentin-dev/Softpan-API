
using Mapster;
using Softpan.Application.DTOs;
using Softpan.Application.Exceptions;
using Softpan.Application.Interfaces;
using Softpan.Domain.Entities;
using Softpan.Domain.Interfaces;

namespace Softpan.Application.Services;

public class ClienteService(IClienteRepository clienteRepository,IRedisCacheService cacheService) : IClienteService
{

    public async Task<ClienteDto?> GetClientByIdAsync(int id)
    {
        var cacheCliente = await cacheService.GetAsync<ClienteDto>($"cliente:{id}");

        if (cacheCliente != null)
        {
            return cacheCliente;
        }

        var cliente = await clienteRepository.GetByIdAsync(id);
        if (cliente == null)
        {
            throw new NotFoundException("Cliente", id);
        }
        var dto = MapToDto(cliente);
        await cacheService.SetAsync($"cliente:{id}", dto, TimeSpan.FromMinutes(10));
        return dto;
    }
    public async Task<IEnumerable<ClienteDto>> GetAllClientsAsync()
    {
        var cacheCliente = await cacheService.GetAsync<IEnumerable<ClienteDto>>("clientes:todos");
        if (cacheCliente != null)
        {
            return cacheCliente;
        }
        var clientes = await clienteRepository.GetAllAsync();
        var dto = clientes.Select(MapToDto).ToList();
        await cacheService.SetAsync("clientes:todos", dto, TimeSpan.FromMinutes(10));
        return dto;
        
    }

    public async Task<ClienteDto> CreateClientAsync(CreateClienteDto clienteDto)
    {

        var cliente = clienteDto.Adapt<Cliente>();
        var createCliente = await clienteRepository.CreateAsync(cliente);
        
        await cacheService.RemoveAsync("clientes:todos");
        await cacheService.RemoveAsync("clientes:con-deuda");

        return MapToDto(createCliente);
        
    }
    public async Task<ClienteDto> UpdateClientAsync(int id, UpdateClienteDto clienteDto)
    {
        if (id != clienteDto.Id)
        {
            throw new BadRequestException("El ID de la URL no coincide con el ID del body");
        }

        var existingCliente = await clienteRepository.GetByIdAsync(id);
        if (existingCliente == null)
        {
            throw new NotFoundException("Cliente", id);
        }
        clienteDto.Adapt(existingCliente);
        var updateCliente = await clienteRepository.UpdateAsync(existingCliente);
        await cacheService.RemoveAsync("clientes:todos");
        await cacheService.RemoveAsync("clientes:con-deuda");
        await cacheService.RemoveAsync($"cliente:{id}");

        return MapToDto(updateCliente);
    }

    public async Task<bool> DeleteClientAsync(int id)
    {
        var result = await clienteRepository.DeleteAsync(id);

        if (result)
        {
            await cacheService.RemoveAsync($"cliente:{id}");
            await cacheService.RemoveAsync("clientes:todos");
            await cacheService.RemoveAsync("clientes:con-deuda");
        }

        return result;
    }
    public async Task<IEnumerable<ClienteDto>> GetClientsWithDebtsAsync()
    {
        var cacheCliente = await cacheService.GetAsync<IEnumerable<ClienteDto>>("clientes:con-deuda");

        if (cacheCliente != null)
        {
            return cacheCliente;
        }
        var clientes = await clienteRepository.GetClientsWithDebts();
        var dto = clientes.Select(MapToDto).ToList();
        await cacheService.SetAsync("clientes:con-deuda", dto, TimeSpan.FromMinutes(5));
        return dto;
    }


    public async Task<ClienteDto> GetClienteMostradorAsync()
    {
        var clientes = await GetAllClientsAsync();
        var mostrador = clientes.FirstOrDefault(c => c.Nombre == "Cliente Mostrador");
        
        if (mostrador == null)
        {
            throw new NotFoundException("Cliente Mostrador no encontrado. Ejecutar script SQL_CLIENTE_MOSTRADOR.sql");
        }

        return mostrador;
    }

    private static ClienteDto MapToDto(Cliente cliente) => cliente.Adapt<ClienteDto>();
}
