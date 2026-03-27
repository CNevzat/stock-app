using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace StockApp.Services;

public interface IImageService
{
    Task<string?> SaveImageAsync(IFormFile? imageFile, int productId);
    void DeleteImage(string? imagePath);
}

public class ImageService : IImageService
{
    private readonly string _imagesPath;
    // Tüm yaygın resim formatları
    private readonly string[] _allowedExtensions = { 
        ".jpg", ".jpeg", ".png", ".gif", ".webp", 
        ".bmp", ".tiff", ".tif", ".svg", ".ico",
        ".heic", ".heif", ".avif", ".jfif", ".pjpeg",
        ".pjp", ".jpe", ".jif", ".jp2", ".j2k", ".jpf"
    };
    private const long _maxFileSize = 5 * 1024 * 1024; // 5MB
    /// <summary>Uzun kenar (px); oran korunur, büyük görseller bu sınırın içine sığdırılır.</summary>
    private const int MaxImageDimension = 1600;

    public ImageService(IWebHostEnvironment environment)
    {
        _imagesPath = Path.Combine(environment.ContentRootPath, "wwwroot", "images");
        
        // Images klasörünü oluştur
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

        // Dosya uzantısı kontrolü
        var extension = Path.GetExtension(imageFile.FileName).ToLowerInvariant();
        if (!_allowedExtensions.Contains(extension))
        {
            throw new InvalidOperationException($"Geçersiz dosya uzantısı. İzin verilen uzantılar: {string.Join(", ", _allowedExtensions)}");
        }

        // Dosya boyutu kontrolü
        if (imageFile.Length > _maxFileSize)
        {
            throw new InvalidOperationException($"Dosya boyutu çok büyük. Maksimum boyut: {_maxFileSize / (1024 * 1024)}MB");
        }

        // Benzersiz dosya adı oluştur: product_{id}_{guid}.ext
        var fileName = $"product_{productId}_{Guid.NewGuid():N}{extension}";
        var filePath = Path.Combine(_imagesPath, fileName);

        await using (var ms = new MemoryStream())
        {
            await imageFile.CopyToAsync(ms);
            var bytes = ms.ToArray();

            try
            {
                using var loadStream = new MemoryStream(bytes);
                using var image = await Image.LoadAsync(loadStream);

                var needsResize = image.Width > MaxImageDimension || image.Height > MaxImageDimension;
                if (!needsResize)
                {
                    await File.WriteAllBytesAsync(filePath, bytes);
                }
                else
                {
                    image.Mutate(ctx => ctx.Resize(new ResizeOptions
                    {
                        Size = new Size(MaxImageDimension, MaxImageDimension),
                        Mode = ResizeMode.Max,
                    }));

                    await image.SaveAsync(filePath);
                }
            }
            catch (UnknownImageFormatException)
            {
                throw new InvalidOperationException(
                    "Görüntü dosyası okunamadı. Desteklenen raster formatları kullanın (ör. JPEG, PNG, WebP). SVG vektör dosyaları bu işlem için uygun değildir.");
            }
        }

        // Relative path döndür (/images/filename)
        return $"/images/{fileName}";
    }

    public void DeleteImage(string? imagePath)
    {
        if (string.IsNullOrEmpty(imagePath))
        {
            return;
        }

        try
        {
            var fileName = Path.GetFileName(imagePath);
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
}
