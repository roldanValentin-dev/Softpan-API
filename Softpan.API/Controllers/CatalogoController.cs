using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Softpan.Application.Interfaces;

namespace Softpan.API.Controllers;

[AllowAnonymous]
[ApiController]
[Route("api/catalogo")]
public class CatalogoController(IProductoService productoService) : ControllerBase
{
    [HttpGet("productos")]
    public async Task<IActionResult> GetProductos()
    {
        var productos = await productoService.GetProductosActivosAsync();
        return Ok(productos);
    }

    [HttpGet("productos/{id}")]
    public async Task<IActionResult> GetProductoById(int id)
    {
        var producto = await productoService.GetProductoByIdAsync(id);
        return Ok(producto);
    }

    [HttpGet("productos/categoria/{categoria}")]
    public async Task<IActionResult> GetProductosByCategoria(string categoria)
    {
        var productos = await productoService.GetProductosByCategoriaAsync(categoria);
        return Ok(productos);
    }

    [HttpGet("categorias")]
    public async Task<IActionResult> GetCategorias()
    {
        var categorias = await productoService.GetCategoriasAsync();
        return Ok(categorias);
    }
}
