using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Softpan.Application.DTOs;
using Softpan.Application.Interfaces;

namespace Softpan.API.Controllers;

[Authorize]
[ApiController]
[Route("api/ventas")]
public class VentasController(IVentaService ventaService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var ventas = await ventaService.GetAllVentasAsync();
        return Ok(ventas);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var venta = await ventaService.GetVentaByIdAsync(id);
        if (venta == null)
            return NotFound(new { message = "Venta no encontrada" });

        return Ok(venta);
    }

    [HttpGet("cliente/{clienteId}")]
    public async Task<IActionResult> GetByCliente(int clienteId)
    {
        var ventas = await ventaService.GetAllVentasByClienteAsync(clienteId);
        return Ok(ventas);
    }

    [HttpGet("pendientes")]
    public async Task<IActionResult> GetPendientes()
    {
        var ventas = await ventaService.GetAllVentasPendientesAsync();
        return Ok(ventas);
    }

    [HttpGet("pagadas")]
    public async Task<IActionResult> GetPagadas()
    {
        var ventas = await ventaService.GetAllVentasPagadasAsync();
        return Ok(ventas);
    }

    [HttpGet("fecha")]
    public async Task<IActionResult> GetByFecha([FromQuery] DateTime fechaInicio, [FromQuery] DateTime fechaFin)
    {
        var ventas = await ventaService.GetAllVentasByFechaAsync(fechaInicio, fechaFin);
        return Ok(ventas);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateVentaDto dto)
    {
        var venta = await ventaService.CreateVentaAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = venta.Id }, venta);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await ventaService.DeleteVentaAsync(id);
        if (!result)
            return NotFound(new { message = "Venta no encontrada" });

        return NoContent();
    }
}