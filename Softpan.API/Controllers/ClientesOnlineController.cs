using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Softpan.Application.DTOs;
using Softpan.Application.Interfaces;
using System.Security.Claims;

namespace Softpan.API.Controllers;

[Authorize]
[ApiController]
[Route("api/clientes-online")]
public class ClientesOnlineController(IClienteOnlineService clienteOnlineService) : ControllerBase
{
    [HttpGet("perfil")]
    public async Task<IActionResult> GetPerfil()
    {
        var usuarioIdentityId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var perfil = await clienteOnlineService.GetPerfilAsync(usuarioIdentityId!);
        return Ok(perfil);
    }

    [HttpPut("perfil")]
    public async Task<IActionResult> UpdatePerfil([FromBody] UpdateClienteOnlineDto dto)
    {
        var usuarioIdentityId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var perfil = await clienteOnlineService.UpdateAsync(usuarioIdentityId!, dto);
        return Ok(perfil);
    }
}
