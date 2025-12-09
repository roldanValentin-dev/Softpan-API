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
        return Ok(cliente);
    }

    [HttpGet("con-deudas")]
    public async Task<IActionResult> GetConDeudas()
    {
        var clientes = await clienteService.GetClientsWithDebtsAsync();
        return Ok(clientes);
    }

    [HttpGet("mostrador")]
    public async Task<IActionResult> GetClienteMostrador()
    {
        var mostrador = await clienteService.GetClienteMostradorAsync();
        return Ok(mostrador);
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
        var cliente = await clienteService.UpdateClientAsync(id, dto);
        return Ok(cliente);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await clienteService.DeleteClientAsync(id);
        return NoContent();
    }
}
