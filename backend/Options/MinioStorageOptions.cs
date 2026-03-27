namespace StockApp.Options;

// MinIO nesne depolama ayarları (Endpoint + tarayıcı için BaseUrl ayrı).
public class MinioStorageOptions
{
    public const string SectionName = "Minio";

    // Yerel disk kullanılsın mı; false iken wwwroot/images.
    public bool Enabled { get; set; }

    // Örn: localhost:9000 veya minio:9000 (şema yok).
    public string Endpoint { get; set; } = string.Empty;

    public string AccessKey { get; set; } = string.Empty;

    public string SecretKey { get; set; } = string.Empty;

    // Bucket adı.
    public string Bucket { get; set; } = "stockapp-images";

    // Tarayıcıda kullanılacak kök URL, bucket içermez. Örn: http://localhost:9000
    public string BaseUrl { get; set; } = string.Empty;

    // HTTPS için true; yerel http:// için false.
    public bool UseSSL { get; set; }
}
