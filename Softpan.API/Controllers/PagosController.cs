using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Softpan.Application.DTOs;
using Softpan.Application.Interfaces;
using Softpan.Domain.Enums;

namespace Softpan.API.Controllers;

[Authorize]
[ApiController]
[Route("api/pagos")]
public class PagosController(IPagoService pagoService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var pagos = await pagoService.GetAllPagosAsync();
        return Ok(pagos);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var pago = await pagoService.GetPagoByIdAsync(id);
        return Ok(pago);
    }

    [HttpGet("{id}/detalle")]
    public async Task<IActionResult> GetDetalle(int id)
    {
        var pago = await pagoService.GetPagoDetalleByIdAsync(id);
        return Ok(pago);
    }

    [HttpGet("cliente/{clienteId}")]
    public async Task<IActionResult> GetByCliente(int clienteId)
    {
        var pagos = await pagoService.GetPagosByClienteAsync(clienteId);
        return Ok(pagos);
    }

    [HttpGet("tipo/{tipo}")]
    public async Task<IActionResult> GetByTipo(TipoPagoEnum tipo)
    {
        var pagos = await pagoService.GetPagosByTipoAsync(tipo);
        return Ok(pagos);
    }

    [HttpGet("fecha")]
    public async Task<IActionResult> GetByFecha([FromQuery] DateTime fechaInicio, [FromQuery] DateTime fechaFin)
    {
        var pagos = await pagoService.GetPagosByFechaAsync(fechaInicio, fechaFin);
        return Ok(pagos);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreatePagoDto dto)
    {
        var pago = await pagoService.CreatePagoAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = pago.Id }, pago);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await pagoService.DeletePagoAsync(id);
        return NoContent();
    }
}
