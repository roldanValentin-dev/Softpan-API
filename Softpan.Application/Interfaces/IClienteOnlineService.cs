
using Softpan.Application.DTOs;

namespace Softpan.Application.Interfaces;

public interface IClienteOnlineService
{
    Task<ClienteOnlineDto> GetByIdAsync(int id);
    Task<ClienteOnlineDto> GetByUsuarioIdentityIdAsync(string usuarioIdentityId);
    Task<ClienteOnlineDto> GetPerfilAsync(string usuarioIdentityId);
    Task<ClienteOnlineDto> CreateAsync(RegisterClienteOnlineDto dto, string usuarioIdentityId);
    Task<ClienteOnlineDto> UpdateAsync(string usuarioIdentityId, UpdateClienteOnlineDto dto);
}
