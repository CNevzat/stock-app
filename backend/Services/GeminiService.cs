using Google.GenAI;
using Google.GenAI.Types;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StockApp.Options;

namespace StockApp.Services;

public interface IGeminiService
{
    Task<GeminiTextResult> GenerateTextAsync(string prompt, CancellationToken cancellationToken = default);
}

public record GeminiTextResult(bool Success, string Message, string? Model = null, bool IsConfigured = true);

public class GeminiService : IGeminiService
{
    private readonly IOptions<GeminiOptions> _options;
    private readonly ILogger<GeminiService> _logger;

    public GeminiService(IOptions<GeminiOptions> options, ILogger<GeminiService> logger)
    {
        _options = options;
        _logger = logger;
    }

    public async Task<GeminiTextResult> GenerateTextAsync(string prompt, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(prompt))
        {
            return new GeminiTextResult(false, "İstek metni boş olamaz.");
        }

        var apiKey = ResolveApiKey();
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            _logger.LogWarning("Gemini API anahtarı bulunamadı. Ortam değişkeni 'GEMINI_API_KEY' veya konfigürasyondaki Gemini:ApiKey ayarını sağlayın.");
            return new GeminiTextResult(false, "Yapay zekâ raporu oluşturmak için Gemini API anahtarı yapılandırılmamış.", null, false);
        }

        var optionsSnapshot = _options.Value;

        string? previousGeminiKey = null;
        string? previousGoogleKey = null;
        string? previousBaseUrl = null;
        var geminiKeyOverridden = false;
        var googleKeyOverridden = false;
        var baseUrlOverridden = false;

        try
        {
            previousGeminiKey = System.Environment.GetEnvironmentVariable("GEMINI_API_KEY");
            if (string.IsNullOrWhiteSpace(previousGeminiKey))
            {
                System.Environment.SetEnvironmentVariable("GEMINI_API_KEY", apiKey);
                geminiKeyOverridden = true;
            }

            previousGoogleKey = System.Environment.GetEnvironmentVariable("GOOGLE_API_KEY");
            if (string.IsNullOrWhiteSpace(previousGoogleKey))
            {
                System.Environment.SetEnvironmentVariable("GOOGLE_API_KEY", apiKey);
                googleKeyOverridden = true;
            }

            if (!string.IsNullOrWhiteSpace(optionsSnapshot.ApiEndpoint))
            {
                previousBaseUrl = System.Environment.GetEnvironmentVariable("GOOGLE_API_BASE_URL");
                System.Environment.SetEnvironmentVariable("GOOGLE_API_BASE_URL", optionsSnapshot.ApiEndpoint);
                baseUrlOverridden = true;
            }

            var client = new Client();
            var response = await client.Models.GenerateContentAsync(
                model: optionsSnapshot.Model,
                contents: prompt);

            var text = response?.Candidates?
                .SelectMany(candidate => candidate.Content?.Parts ?? Enumerable.Empty<Part>())
                .Select(part => part.Text)
                .FirstOrDefault(partText => !string.IsNullOrWhiteSpace(partText));

            if (string.IsNullOrWhiteSpace(text))
            {
                _logger.LogWarning("Gemini API yanıtında metin bulunamadı. Response: {@Response}", response);
                return new GeminiTextResult(false, "Gemini modelinden anlamlı bir yanıt alınamadı.", optionsSnapshot.Model);
            }

            return new GeminiTextResult(true, text.Trim(), optionsSnapshot.Model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Gemini API çağrısı sırasında beklenmeyen bir hata oluştu.");
            return new GeminiTextResult(false, "Gemini servisine bağlanırken beklenmeyen bir hata oluştu.");
        }
        finally
        {
            if (geminiKeyOverridden)
            {
                System.Environment.SetEnvironmentVariable("GEMINI_API_KEY", previousGeminiKey);
            }

            if (googleKeyOverridden)
            {
                System.Environment.SetEnvironmentVariable("GOOGLE_API_KEY", previousGoogleKey);
            }

            if (baseUrlOverridden)
            {
                System.Environment.SetEnvironmentVariable("GOOGLE_API_BASE_URL", previousBaseUrl);
            }
        }
    }

    private string? ResolveApiKey()
    {
        var configured = _options.Value.ApiKey;
        if (!string.IsNullOrWhiteSpace(configured))
        {
            return configured;
        }

        return System.Environment.GetEnvironmentVariable("GEMINI_API_KEY");
    }
}
