using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Softpan.Application.DTOs;
using Softpan.Application.Interfaces;

namespace Softpan.API.Controllers;

[Authorize]
[ApiController]
[Route("api/clientes")]
public class ClientesController(IClienteService clienteService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var clientes = await clienteService.GetAllClientsAsync();
        return Ok(clientes);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var cliente = await clienteService.GetClientByIdAsync(id);
        if (cliente == null)
            return NotFound(new { message = "Cliente no encontrado" });

        return Ok(cliente);
    }

    [HttpGet("con-deudas")]
    public async Task<IActionResult> GetConDeudas()
    {
        var clientes = await clienteService.GetClientsWithDebtsAsync();
        return Ok(clientes);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateClienteDto dto)
    {
        var cliente = await clienteService.CreateClientAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = cliente.Id }, cliente);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateClienteDto dto)
    {
        if (id != dto.Id)
            return BadRequest(new { message = "El ID de la URL no coincide con el ID del body" });

        var cliente = await clienteService.UpdateClientAsync(dto);
        return Ok(cliente);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await clienteService.DeleteClientAsync(id);
        if (!result)
            return NotFound(new { message = "Cliente no encontrado" });

        return NoContent();
    }
}
