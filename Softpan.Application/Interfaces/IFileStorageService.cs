namespace Softpan.Application.Interfaces;

/// <summary>
/// Servicio para gestionar el almacenamiento de archivos
/// </summary>
public interface IFileStorageService
{
    /// <summary>
    /// Guarda un archivo en el sistema de almacenamiento
    /// </summary>
    /// <param name="fileStream">Stream del archivo a guardar</param>
    /// <param name="fileName">Nombre original del archivo</param>
    /// <param name="folder">Carpeta donde guardar (ej: "productos")</param>
    /// <returns>URL relativa del archivo guardado (ej: "/images/productos/abc123.jpg")</returns>
    Task<string> SaveFileAsync(Stream fileStream, string fileName, string folder);

    /// <summary>
    /// Elimina un archivo del sistema de almacenamiento
    /// </summary>
    /// <param name="fileUrl">URL relativa del archivo (ej: "/images/productos/abc123.jpg")</param>
    Task<bool> DeleteFileAsync(string fileUrl);

    /// <summary>
    /// Valida si un archivo es una imagen válida
    /// </summary>
    /// <param name="fileName">Nombre del archivo</param>
    /// <param name="fileSize">Tamaño en bytes</param>
    /// <returns>Tupla con (esValido, mensajeError)</returns>
    (bool isValid, string errorMessage) ValidateImageFile(string fileName, long fileSize);
}
