using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Softpan.Application.DTOs;
using Softpan.Application.Interfaces;
using System.Security.Claims;

namespace Softpan.API.Controllers;

[Authorize]
[ApiController]
[Route("api/carrito")]
public class CarritoController(IPedidoService pedidoService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetCarrito()
    {
        var usuarioId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var carrito = await pedidoService.ObtenerOCrearCarritoAsync(usuarioId!);
        return Ok(carrito);
    }
    [HttpPost("items")]
    public async Task<IActionResult> AgregarItem([FromBody] AgregarItemCarritoDto dto)
    {
        var usuarioId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var carrito = await pedidoService.AgregarItemAlCarritoAsync(usuarioId!, dto.ProductoId, dto.Cantidad);
        return Ok(carrito);
    }
    [HttpPut("items/{productoId}")]
    public async Task<IActionResult> ActualizarItem(int productoId, [FromBody] ActualizarItemCarritoDto dto)
    {
        var usuarioId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var carrito = await pedidoService.ActualizarItemEnCarritoAsync(usuarioId!, productoId, dto.Cantidad);
        return Ok(carrito);
    }
    [HttpDelete("items/{productoId}")]
    public async Task<IActionResult> RemoverItem(int productoId)
    {
        var usuarioId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        await pedidoService.RemoverItemDelCarritoAsync(usuarioId!, productoId);
        return NoContent();
    }
    [HttpDelete]
    public async Task<IActionResult> LimpiarCarrito()
    {
        var usuarioId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var carrito = await pedidoService.LimpiarCarritoAsync(usuarioId!);
        return Ok(carrito);
    }
    [HttpPost("checkout")]
    public async Task<IActionResult> ProcesarCheckout([FromBody] ProcesarCheckoutDto dto)
    {
        var usuarioId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var pedido = await pedidoService.ProcesarCheckoutDesdeCarritoAsync(usuarioId!, dto);
        return Ok(pedido);
    }
}