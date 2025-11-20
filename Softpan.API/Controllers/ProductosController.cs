using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Softpan.Application.DTOs;
using Softpan.Application.Interfaces;

namespace Softpan.API.Controllers;

[Authorize]
[ApiController]
[Route("api/productos")]
public class ProductosController(IProductoService productoService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var productos = await productoService.GetAllProductosAsync();
        return Ok(productos);
    }

    [HttpGet("activos")]
    public async Task<IActionResult> GetActivos()
    {
        var productos = await productoService.GetProductosActivosAsync();
        return Ok(productos);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var producto = await productoService.GetProductoByIdAsync(id);
        if (producto == null)
            return NotFound(new { message = "Producto no encontrado" });

        return Ok(producto);
    }

    [HttpGet("{id}/detalle")]
    public async Task<IActionResult> GetDetalle(int id)
    {
        var producto = await productoService.GetProductoDetalleByIdAsync(id);
        if (producto == null)
            return NotFound(new { message = "Producto no encontrado" });

        return Ok(producto);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateProductoDto dto)
    {
        var producto = await productoService.CreateProductoAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = producto.Id }, producto);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateProductoDto dto)
    {
        if (id != dto.Id)
            return BadRequest(new { message = "El ID de la URL no coincide con el ID del body" });

        var producto = await productoService.UpdateProductoAsync(id,dto);
        return Ok(producto);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await productoService.DeleteProductoAsync(id);
        if (!result)
            return NotFound(new { message = "Producto no encontrado" });

        return NoContent();
    }
}