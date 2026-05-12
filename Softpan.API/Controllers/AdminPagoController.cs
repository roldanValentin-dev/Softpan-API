using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Softpan.Application.DTOs;
using Softpan.Application.Interfaces;

namespace Softpan.API.Controllers;

[Authorize(Roles = "Admin")]
[ApiController]
[Route("api/admin")]
public class AdminPagoController(IAdminPagoService adminPagoService) : ControllerBase
{
    // ========================================================================
    // DESCUENTO
    // ========================================================================
    [HttpGet("configuracion/descuento")]
    public async Task<IActionResult> GetDescuento()
    {
        var descuento = await adminPagoService.GetDescuentoAsync();
        return Ok(descuento);
    }

    [HttpPut("configuracion/descuento")]
    public async Task<IActionResult> UpdateDescuento([FromBody] UpdateConfiguracionPagoDto dto)
    {
        if (!decimal.TryParse(dto.Valor, out var porcentaje))
            return BadRequest(new { message = "El valor debe ser un número válido" });

        var descuento = await adminPagoService.UpdateDescuentoAsync(porcentaje);
        return Ok(descuento);
    }

    // ========================================================================
    // DATOS BANCARIOS
    // ========================================================================
    [HttpGet("datos-bancarios")]
    public async Task<IActionResult> GetDatosBancarios()
    {
        var datos = await adminPagoService.GetDatosBancariosAsync();
        return Ok(datos);
    }

    [HttpPost("datos-bancarios")]
    public async Task<IActionResult> CreateDatosBancarios([FromBody] CreateDatosBancariosDto dto)
    {
        var datos = await adminPagoService.CreateDatosBancariosAsync(dto);
        return CreatedAtAction(nameof(GetDatosBancarios), datos);
    }

    [HttpPut("datos-bancarios/{id}")]
    public async Task<IActionResult> UpdateDatosBancarios(int id, [FromBody] UpdateDatosBancariosDto dto)
    {
        var datos = await adminPagoService.UpdateDatosBancariosAsync(id, dto);
        return Ok(datos);
    }

    [HttpDelete("datos-bancarios/{id}")]
    public async Task<IActionResult> DeleteDatosBancarios(int id)
    {
        await adminPagoService.DeleteDatosBancariosAsync(id);
        return NoContent();
    }

    // ========================================================================
    // DIRECCIÓN DE RETIRO
    // ========================================================================
    [AllowAnonymous]
    [HttpGet("direccion-retiro")]
    public async Task<IActionResult> GetDireccionRetiro()
    {
        var direccion = await adminPagoService.GetDireccionRetiroAsync();
        return Ok(direccion);
    }

    [HttpPut("direccion-retiro")]
    public async Task<IActionResult> UpdateDireccionRetiro([FromBody] UpdateDireccionRetiroDto dto)
    {
        var direccion = await adminPagoService.UpdateDireccionRetiroAsync(dto);
        return Ok(direccion);
    }

    // ========================================================================
    // PEDIDOS PENDIENTES
    // ========================================================================
    [HttpGet("pedidos/pendientes-pago")]
    public async Task<IActionResult> GetPedidosPendientesPago()
    {
        var pedidos = await adminPagoService.GetPedidosPendientesPagoAsync();
        return Ok(pedidos);
    }

    [HttpPost("pedidos/{id}/confirmar-pago")]
    public async Task<IActionResult> ConfirmarPago(int id)
    {
        var pedido = await adminPagoService.ConfirmarPagoPedidoAsync(id);
        return Ok(pedido);
    }
}
