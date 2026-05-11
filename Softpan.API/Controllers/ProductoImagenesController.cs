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
    private readonly IFileStorageService _fileStorageService;

    public ProductoImagenesController(
        IProductoImagenService imagenService,
        IFileStorageService fileStorageService)
    {
        _imagenService = imagenService;
        _fileStorageService = fileStorageService;
    }

    /// <summary>
    /// Obtiene todas las imágenes de un producto (público)
    /// </summary>
    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> GetImagenes(int productoId)
    {
        var imagenes = await _imagenService.GetImagenesByProductoIdAsync(productoId);
        return Ok(imagenes);
    }

    /// <summary>
    /// Sube una nueva imagen para un producto
    /// Flujo: Recibir archivo → Validar → Guardar en disco → Crear registro en BD
    /// </summary>
    /// <param name="productoId">ID del producto</param>
    /// <param name="file">Archivo de imagen (form-data)</param>
    /// <param name="orden">Orden de visualización (opcional, default 0)</param>
    /// <param name="esPrincipal">Si es la imagen principal (opcional, default false)</param>
    [HttpPost]
    [Consumes("multipart/form-data")] // Indica que recibe archivos
    public async Task<IActionResult> UploadImagen(
        int productoId,
        [FromForm] IFormFile file, // Archivo desde el formulario
        [FromForm] int orden = 0,
        [FromForm] bool esPrincipal = false)
    {
        // 1. Validar que se envió un archivo
        if (file == null || file.Length == 0)
            return BadRequest("No se proporcionó ningún archivo");

        // 2. Validar que el archivo sea una imagen válida
        var (isValid, errorMessage) = _fileStorageService.ValidateImageFile(file.FileName, file.Length);
        if (!isValid)
            return BadRequest(errorMessage);

        // 3. Guardar el archivo en el servidor
        // Retorna la URL relativa (ej: /images/productos/20240115_abc123.jpg)
        string imageUrl;
        using (var stream = file.OpenReadStream())
        {
            imageUrl = await _fileStorageService.SaveFileAsync(stream, file.FileName, "productos");
        }

        // 4. Crear el registro en la base de datos
        var dto = new CreateProductoImagenDto
        {
            Url = imageUrl,
            Orden = orden,
            EsPrincipal = esPrincipal
        };

        var imagen = await _imagenService.CreateImagenAsync(productoId, dto);

        // 5. Retornar la imagen creada
        return CreatedAtAction(nameof(GetImagenes), new { productoId }, imagen);
    }

    /// <summary>
    /// Actualiza el orden y si es principal de una imagen
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateImagen(int id, [FromBody] UpdateProductoImagenDto dto)
    {
        var imagen = await _imagenService.UpdateImagenAsync(id, dto);
        return Ok(imagen);
    }

    /// <summary>
    /// Elimina una imagen (registro en BD y archivo físico)
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteImagen(int id)
    {
        await _imagenService.DeleteImagenAsync(id);
        return NoContent();
    }
}
