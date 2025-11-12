namespace StockApp.Options;

public class GeminiOptions
{
    public const string SectionName = "Gemini";

    /// <summary>
    /// Varsayılan model adı. Dilerseniz appsettings veya user-secrets üzerinden override edebilirsiniz.
    /// </summary>
    public string Model { get; set; } = "gemini-2.5-flash";

    /// <summary>
    /// Opsiyonel API endpoint. Boş bırakılırsa SDK varsayılan Google endpoint'ini kullanır.
    /// Örn: https://generativelanguage.googleapis.com/v1
    /// </summary>
    public string? ApiEndpoint { get; set; } = "https://generativelanguage.googleapis.com/v1";

    /// <summary>
    /// Konfigürasyondan okunacak API anahtarı (opsiyonel). Eğer null ise ortam değişkeni
    /// GEMINI_API_KEY kullanılacaktır.
    /// </summary>
    public string? ApiKey { get; set; }
}


