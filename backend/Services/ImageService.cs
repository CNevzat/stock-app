using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Bmp;
using SixLabors.ImageSharp.Formats.Gif;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Formats.Tiff;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Processing;
using StockApp.Options;

namespace StockApp.Services;

public interface IImageService
{
    Task<string?> SaveImageAsync(IFormFile? imageFile, int productId);
    Task DeleteImageAsync(string? imagePath);
}

public class ImageService : IImageService
{
    private readonly string _imagesPath;
    private readonly MinioStorageOptions _minioOptions;
    private readonly MinioFileService? _minio;

    private readonly string[] _allowedExtensions = {
        ".jpg", ".jpeg", ".png", ".gif", ".webp",
        ".bmp", ".tiff", ".tif", ".svg", ".ico",
        ".heic", ".heif", ".avif", ".jfif", ".pjpeg",
        ".pjp", ".jpe", ".jif", ".jp2", ".j2k", ".jpf"
    };

    private const long _maxFileSize = 5 * 1024 * 1024;
    private const int MaxImageDimension = 1600;

    public ImageService(
        IWebHostEnvironment environment,
        IOptions<MinioStorageOptions> minioOptions,
        IServiceProvider services)
    {
        _minioOptions = minioOptions.Value;
        _minio = _minioOptions.Enabled ? services.GetService<MinioFileService>() : null;

        _imagesPath = Path.Combine(environment.ContentRootPath, "wwwroot", "images");
        if (!Directory.Exists(_imagesPath))
        {
            Directory.CreateDirectory(_imagesPath);
        }
    }

    public async Task<string?> SaveImageAsync(IFormFile? imageFile, int productId)
    {
        if (imageFile == null || imageFile.Length == 0)
        {
            return null;
        }

        var extension = Path.GetExtension(imageFile.FileName).ToLowerInvariant();
        if (!_allowedExtensions.Contains(extension))
        {
            throw new InvalidOperationException($"Geçersiz dosya uzantısı. İzin verilen uzantılar: {string.Join(", ", _allowedExtensions)}");
        }

        if (imageFile.Length > _maxFileSize)
        {
            throw new InvalidOperationException($"Dosya boyutu çok büyük. Maksimum boyut: {_maxFileSize / (1024 * 1024)}MB");
        }

        if (_minioOptions.Enabled && _minio == null)
        {
            throw new InvalidOperationException("MinIO etkin ancak MinioFileService kayıtlı değil.");
        }

        if (_minioOptions.Enabled && string.IsNullOrWhiteSpace(_minioOptions.BaseUrl))
        {
            throw new InvalidOperationException("MinIO etkinken Minio:BaseUrl yapılandırılmalıdır.");
        }

        if (_minioOptions.Enabled && string.IsNullOrWhiteSpace(_minioOptions.Endpoint))
        {
            throw new InvalidOperationException("MinIO etkinken Minio:Endpoint yapılandırılmalıdır (örn. localhost:9000).");
        }

        var fileName = $"product_{productId}_{Guid.NewGuid():N}{extension}";

        await using var ms = new MemoryStream();
        await imageFile.CopyToAsync(ms);
        var rawBytes = ms.ToArray();

        byte[] finalBytes;
        try
        {
            finalBytes = await BuildProcessedImageBytesAsync(rawBytes, extension);
        }
        catch (UnknownImageFormatException)
        {
            throw new InvalidOperationException(
                "Görüntü dosyası okunamadı. Desteklenen raster formatları kullanın (ör. JPEG, PNG, WebP). SVG vektör dosyaları bu işlem için uygun değildir.");
        }

        if (_minioOptions.Enabled && _minio != null)
        {
            var contentType = GetContentType(extension);
            await using var upload = new MemoryStream(finalBytes);
            return await _minio.UploadObjectAsync(upload, fileName, finalBytes.Length, contentType);
        }

        var filePath = Path.Combine(_imagesPath, fileName);
        await File.WriteAllBytesAsync(filePath, finalBytes);
        return $"/images/{fileName}";
    }

    public async Task DeleteImageAsync(string? imagePath)
    {
        if (string.IsNullOrEmpty(imagePath))
        {
            return;
        }

        try
        {
            if (_minioOptions.Enabled && _minio != null && imagePath.StartsWith("http", StringComparison.OrdinalIgnoreCase))
            {
                await _minio.DeleteObjectByUrlAsync(imagePath);
                return;
            }

            var fileName = Path.GetFileName(imagePath);
            if (string.IsNullOrEmpty(fileName))
            {
                return;
            }

            var filePath = Path.Combine(_imagesPath, fileName);
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Resim silme hatası: {ex.Message}");
        }
    }

    private static async Task<byte[]> BuildProcessedImageBytesAsync(byte[] rawBytes, string extension)
    {
        using var loadStream = new MemoryStream(rawBytes);
        using var image = await Image.LoadAsync(loadStream);

        var needsResize = image.Width > MaxImageDimension || image.Height > MaxImageDimension;
        if (!needsResize)
        {
            return rawBytes;
        }

        image.Mutate(ctx => ctx.Resize(new ResizeOptions
        {
            Size = new Size(MaxImageDimension, MaxImageDimension),
            Mode = ResizeMode.Max,
        }));

        await using var outMs = new MemoryStream();
        await SaveWithEncoderAsync(image, extension, outMs);
        return outMs.ToArray();
    }

    private static Task SaveWithEncoderAsync(Image image, string extension, Stream destination)
    {
        return extension switch
        {
            ".jpg" or ".jpeg" or ".jfif" or ".pjpeg" or ".pjp" or ".jpe" or ".jif" =>
                image.SaveAsJpegAsync(destination, new JpegEncoder { Quality = 90 }),
            ".png" or ".ico" =>
                image.SaveAsPngAsync(destination),
            ".webp" =>
                image.SaveAsWebpAsync(destination),
            ".avif" or ".heic" or ".heif" =>
                image.SaveAsJpegAsync(destination, new JpegEncoder { Quality = 90 }),
            ".gif" =>
                image.SaveAsGifAsync(destination),
            ".bmp" =>
                image.SaveAsBmpAsync(destination),
            ".tiff" or ".tif" =>
                image.SaveAsTiffAsync(destination),
            _ =>
                image.SaveAsJpegAsync(destination, new JpegEncoder { Quality = 90 }),
        };
    }

    private static string GetContentType(string extension) =>
        extension.ToLowerInvariant() switch
        {
            ".jpg" or ".jpeg" or ".jfif" or ".pjpeg" or ".pjp" or ".jpe" or ".jif" => "image/jpeg",
            ".png" => "image/png",
            ".webp" or ".avif" => "image/webp",
            ".gif" => "image/gif",
            ".bmp" => "image/bmp",
            ".tiff" or ".tif" => "image/tiff",
            ".svg" => "image/svg+xml",
            ".ico" => "image/x-icon",
            _ => "application/octet-stream",
        };
}
