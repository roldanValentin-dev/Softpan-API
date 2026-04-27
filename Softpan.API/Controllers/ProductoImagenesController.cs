using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Softpan.Application.DTOs;
using Softpan.Application.Interfaces;

namespace Softpan.API.Controllers;

[Authorize]
[ApiController]
[Route("api/productos/{productoId}/imagenes")]
public class ProductoImagenesController : ControllerBase
{
    private readonly IProductoImagenService _imagenService;

    public ProductoImagenesController(IProductoImagenService imagenService)
    {
        _imagenService = imagenService;
    }

    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> GetImagenes(int productoId)
    {
        var imagenes = await _imagenService.GetImagenesByProductoIdAsync(productoId);
        return Ok(imagenes);
    }

    [HttpPost]
    public async Task<IActionResult> CreateImagen(int productoId, [FromBody] CreateProductoImagenDto dto)
    {
        var imagen = await _imagenService.CreateImagenAsync(productoId, dto);
        return CreatedAtAction(nameof(GetImagenes), new { productoId }, imagen);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateImagen(int id, [FromBody] UpdateProductoImagenDto dto)
    {
        var imagen = await _imagenService.UpdateImagenAsync(id, dto);
        return Ok(imagen);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteImagen(int id)
    {
        await _imagenService.DeleteImagenAsync(id);
        return NoContent();
    }
}
