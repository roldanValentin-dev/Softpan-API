using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Softpan.Application.DTOs;
using Softpan.Application.Interfaces;
using Softpan.Domain.Enums;
using System.Security.Claims;

namespace Softpan.API.Controllers;

[Authorize]
[ApiController]
[Route("api/pedidos")]
public class PedidosController(IPedidoService pedidoService) : ControllerBase
{
    // ========== ENDPOINTS PARA CLIENTE ==========

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreatePedidoDto dto)
    {
        var usuarioIdentityId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var pedido = await pedidoService.CreatePedidoAsync(usuarioIdentityId!, dto);
        return CreatedAtAction(nameof(GetById), new { id = pedido.Id }, pedido);
    }

    [HttpGet("mis-pedidos")]
    public async Task<IActionResult> GetMisPedidos()
    {
        var usuarioIdentityId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var pedidos = await pedidoService.GetMisPedidosAsync(usuarioIdentityId!);
        return Ok(pedidos);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var usuarioIdentityId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var pedido = await pedidoService.GetPedidoByIdAsync(id, usuarioIdentityId!);
        return Ok(pedido);
    }

    // ========== ENDPOINTS PARA ADMIN ==========

    [Authorize(Roles = "Admin")]
    [HttpGet("todos")]
    public async Task<IActionResult> GetTodos()
    {
        var pedidos = await pedidoService.GetAllPedidosAsync();
        return Ok(pedidos);
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("estado/{estadoId}")]
    public async Task<IActionResult> GetByEstado(int estadoId)
    {
        var pedidos = await pedidoService.GetPedidosByEstadoAsync((EstadoPedidoEnum)estadoId);
        return Ok(pedidos);
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("detalle/{id}")]
    public async Task<IActionResult> GetDetalle(int id)
    {
        var pedido = await pedidoService.GetPedidoDetalleByIdAsync(id);
        return Ok(pedido);
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id}/estado")]
    public async Task<IActionResult> UpdateEstado(int id, [FromBody] UpdateEstadoPedidoDto dto)
    {
        var pedido = await pedidoService.UpdateEstadoPedidoAsync(id, dto);
        return Ok(pedido);
    }
}
