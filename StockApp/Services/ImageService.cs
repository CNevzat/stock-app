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

        // Dosyayı kaydet
        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await imageFile.CopyToAsync(stream);
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
            // Log hatası ama işlemi durdurma
            Console.WriteLine($"Resim silme hatası: {ex.Message}");
        }
    }
}
