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

        var o = _options.Value;
        var apiKey = string.IsNullOrWhiteSpace(o.ApiKey)
            ? System.Environment.GetEnvironmentVariable("GEMINI_API_KEY")
            : o.ApiKey;

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            _logger.LogWarning("Gemini API anahtarı bulunamadı. Konfigürasyonda Gemini:ApiKey veya ortam değişkeni GEMINI_API_KEY tanımlayın.");
            return new GeminiTextResult(false, "Yapay zekâ raporu oluşturmak için Gemini API anahtarı yapılandırılmamış.", null, false);
        }

        HttpOptions? httpOptions = string.IsNullOrWhiteSpace(o.ApiEndpoint)
            ? null
            : new HttpOptions { BaseUrl = o.ApiEndpoint };

        try
        {
            using var client = new Client(apiKey: apiKey, httpOptions: httpOptions);
            var response = await client.Models.GenerateContentAsync(
                model: o.Model,
                contents: prompt);

            var text = response?.Candidates?
                .SelectMany(candidate => candidate.Content?.Parts ?? Enumerable.Empty<Part>())
                .Select(part => part.Text)
                .FirstOrDefault(partText => !string.IsNullOrWhiteSpace(partText));

            if (string.IsNullOrWhiteSpace(text))
            {
                _logger.LogWarning("Gemini API yanıtında metin bulunamadı. Response: {@Response}", response);
                return new GeminiTextResult(false, "Gemini modelinden anlamlı bir yanıt alınamadı.", o.Model);
            }

            return new GeminiTextResult(true, text.Trim(), o.Model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Gemini API çağrısı sırasında beklenmeyen bir hata oluştu.");
            return new GeminiTextResult(false, "Gemini servisine bağlanırken beklenmeyen bir hata oluştu.");
        }
    }
}
