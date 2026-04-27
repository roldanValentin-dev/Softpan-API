using Mapster;
using Softpan.Application.DTOs;
using Softpan.Application.Exceptions;
using Softpan.Application.Interfaces;
using Softpan.Domain.Entities;
using Softpan.Domain.Interfaces;

namespace Softpan.Application.Services;

public class ClienteOnlineService(IClienteOnlineRepository clienteOnlineRepository) : IClienteOnlineService
{
    public async Task<ClienteOnlineDto> GetByIdAsync(int id)
    {
        var cliente = await clienteOnlineRepository.GetByIdAsync(id);
        if (cliente == null)
            throw new NotFoundException("ClienteOnline", id);

        return MapToDto(cliente);
    }

    public async Task<ClienteOnlineDto> GetByUsuarioIdentityIdAsync(string usuarioIdentityId)
    {
        var cliente = await clienteOnlineRepository.GetByUsuarioIdentityIdAsync(usuarioIdentityId);
        if (cliente == null)
            throw new NotFoundException("Cliente no encontrado");

        return MapToDto(cliente);
    }

    public async Task<ClienteOnlineDto> GetPerfilAsync(string usuarioIdentityId)
    {
        return await GetByUsuarioIdentityIdAsync(usuarioIdentityId);
    }

    public async Task<ClienteOnlineDto> CreateAsync(RegisterClienteOnlineDto dto, string usuarioIdentityId)
    {
        var existeCliente = await clienteOnlineRepository.ExistsByEmailAsync(dto.Email);
        if (existeCliente)
            throw new BadRequestException("Ya existe un cliente con ese email");

        var cliente = new ClienteOnline
        {
            Nombre = dto.Nombre,
            Apellido = dto.Apellido,
            Email = dto.Email,
            Telefono = dto.Telefono,
            Direccion = dto.Direccion,
            UsuarioIdentityId = usuarioIdentityId,
            FechaRegistro = DateTime.UtcNow,
            Activo = true
        };

        var clienteCreado = await clienteOnlineRepository.CreateAsync(cliente);
        return MapToDto(clienteCreado);
    }

    public async Task<ClienteOnlineDto> UpdateAsync(string usuarioIdentityId, UpdateClienteOnlineDto dto)
    {
        var cliente = await clienteOnlineRepository.GetByUsuarioIdentityIdAsync(usuarioIdentityId);
        if (cliente == null)
            throw new NotFoundException("Cliente no encontrado");

        cliente.Nombre = dto.Nombre;
        cliente.Apellido = dto.Apellido;
        cliente.Telefono = dto.Telefono;
        cliente.Direccion = dto.Direccion;

        var clienteActualizado = await clienteOnlineRepository.UpdateAsync(cliente);
        return MapToDto(clienteActualizado);
    }

    private static ClienteOnlineDto MapToDto(ClienteOnline cliente) => cliente.Adapt<ClienteOnlineDto>();
}
