using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.Extensions.Configuration;
using Softpan.Application.Interfaces;

namespace Softpan.Infrastructure.Services;

public class CloudinaryFileStorageService : IFileStorageService
{
    private readonly Cloudinary _cloudinary;
    private readonly string[] _allowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
    private readonly long _maxFileSize = 5 * 1024 * 1024;

    public CloudinaryFileStorageService(IConfiguration configuration)
    {
        var cloudName = configuration["Cloudinary:CloudName"]
            ?? throw new InvalidOperationException("Cloudinary:CloudName no configurado");
        var apiKey = configuration["Cloudinary:ApiKey"]
            ?? throw new InvalidOperationException("Cloudinary:ApiKey no configurado");
        var apiSecret = configuration["Cloudinary:ApiSecret"]
            ?? throw new InvalidOperationException("Cloudinary:ApiSecret no configurado");

        var account = new Account(cloudName, apiKey, apiSecret);
        _cloudinary = new Cloudinary(account);
    }

    public async Task<string> SaveFileAsync(Stream fileStream, string fileName, string folder)
    {
        var (isValid, errorMessage) = ValidateImageFile(fileName, fileStream.Length);
        if (!isValid)
            throw new InvalidOperationException(errorMessage);

        var uploadParams = new ImageUploadParams
        {
            File = new FileDescription(fileName, fileStream),
            Folder = $"softpan/{folder}",
            UseFilename = true,
            UniqueFilename = true
        };

        var result = await _cloudinary.UploadAsync(uploadParams);

        if (result.Error != null)
            throw new InvalidOperationException($"Error al subir imagen: {result.Error.Message}");

        return result.SecureUrl.ToString();
    }

    public async Task<bool> DeleteFileAsync(string fileUrl)
    {
        try
        {
            var publicId = ExtractPublicId(fileUrl);
            if (string.IsNullOrEmpty(publicId))
                return false;

            var deleteParams = new DeletionParams(publicId);
            var result = await _cloudinary.DestroyAsync(deleteParams);
            return result.Result == "ok";
        }
        catch
        {
            return false;
        }
    }

    public (bool isValid, string errorMessage) ValidateImageFile(string fileName, long fileSize)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        if (!_allowedExtensions.Contains(extension))
            return (false, $"Extensión no permitida. Solo: {string.Join(", ", _allowedExtensions)}");
        if (fileSize > _maxFileSize)
            return (false, $"Máximo permitido: {_maxFileSize / 1024 / 1024} MB");
        if (fileSize == 0)
            return (false, "El archivo está vacío");
        return (true, string.Empty);
    }

    private static string? ExtractPublicId(string fileUrl)
    {
        var uri = new Uri(fileUrl);
        var segments = uri.Segments;
        var softpanIndex = Array.FindIndex(segments, s => s.Contains("softpan"));
        if (softpanIndex < 0) return null;

        var publicIdWithExt = string.Concat(segments[softpanIndex..]);
        return Path.GetFileNameWithoutExtension(publicIdWithExt);
    }
}