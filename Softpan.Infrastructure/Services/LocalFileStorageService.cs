using Microsoft.Extensions.Hosting;
using Softpan.Application.Interfaces;

namespace Softpan.Infrastructure.Services;

/// <summary>
/// Implementación del servicio de almacenamiento local de archivos
/// Guarda archivos en wwwroot/images/{folder}
/// </summary>
public class LocalFileStorageService : IFileStorageService
{
    private readonly IHostEnvironment _environment;
    private readonly string _imagesFolder = "images"; // Carpeta base dentro de wwwroot
    
    // Configuración de validación
    private readonly string[] _allowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
    private readonly long _maxFileSize = 5 * 1024 * 1024; // 5 MB

    public LocalFileStorageService(IHostEnvironment environment)
    {
        _environment = environment;
    }

    /// <summary>
    /// Guarda un archivo en el sistema local
    /// Flujo: Validar → Generar nombre único → Crear carpeta si no existe → Guardar archivo
    /// </summary>
    public async Task<string> SaveFileAsync(Stream fileStream, string fileName, string folder)
    {
        // 1. Validar el archivo
        var (isValid, errorMessage) = ValidateImageFile(fileName, fileStream.Length);
        if (!isValid)
            throw new InvalidOperationException(errorMessage);

        // 2. Generar nombre único para evitar colisiones
        // Formato: {timestamp}_{guid}_{nombreOriginal}
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        var uniqueFileName = $"{DateTime.UtcNow:yyyyMMddHHmmss}_{Guid.NewGuid():N}{extension}";

        // 3. Construir ruta física completa
        // Ejemplo: D:\Repos\Softpan\Softpan.API\wwwroot\images\productos\20240115120000_abc123.jpg
        var folderPath = Path.Combine(_environment.ContentRootPath, "wwwroot", _imagesFolder, folder);
        
        // 4. Crear carpeta si no existe
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        // 5. Ruta completa del archivo
        var filePath = Path.Combine(folderPath, uniqueFileName);

        // 6. Guardar el archivo en disco
        using (var fileStreamOutput = new FileStream(filePath, FileMode.Create))
        {
            await fileStream.CopyToAsync(fileStreamOutput);
        }

        // 7. Retornar URL relativa para guardar en BD
        // Formato: /images/productos/20240115120000_abc123.jpg
        return $"/{_imagesFolder}/{folder}/{uniqueFileName}";
    }

    /// <summary>
    /// Elimina un archivo del sistema local
    /// </summary>
    public Task<bool> DeleteFileAsync(string fileUrl)
    {
        try
        {
            // 1. Convertir URL relativa a ruta física
            // De: /images/productos/abc123.jpg
            // A: D:\Repos\Softpan\Softpan.API\wwwroot\images\productos\abc123.jpg
            var fileName = fileUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
            var filePath = Path.Combine(_environment.ContentRootPath, "wwwroot", fileName);

            // 2. Verificar si el archivo existe
            if (File.Exists(filePath))
            {
                // 3. Eliminar el archivo
                File.Delete(filePath);
                return Task.FromResult(true);
            }

            return Task.FromResult(false);
        }
        catch
        {
            // Si hay error al eliminar, retornar false
            return Task.FromResult(false);
        }
    }

    /// <summary>
    /// Valida que el archivo sea una imagen válida
    /// Verifica: extensión permitida y tamaño máximo
    /// </summary>
    public (bool isValid, string errorMessage) ValidateImageFile(string fileName, long fileSize)
    {
        // 1. Validar extensión del archivo
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        if (!_allowedExtensions.Contains(extension))
        {
            return (false, $"Extensión no permitida. Solo se permiten: {string.Join(", ", _allowedExtensions)}");
        }

        // 2. Validar tamaño del archivo
        if (fileSize > _maxFileSize)
        {
            var maxSizeMB = _maxFileSize / 1024 / 1024;
            return (false, $"El archivo excede el tamaño máximo permitido de {maxSizeMB} MB");
        }

        // 3. Validar que el archivo no esté vacío
        if (fileSize == 0)
        {
            return (false, "El archivo está vacío");
        }

        return (true, string.Empty);
    }
}
