using System.Text;
using StockApp.Services;

namespace StockApp.App.Chat;

public interface IGeminiIntentClassifier
{
    Task<ChatIntent> ClassifyAsync(string question, CancellationToken cancellationToken = default);
}

public class GeminiIntentClassifier : IGeminiIntentClassifier
{
    private readonly IGeminiService _geminiService;
    private static readonly string[] IntentNames = Enum.GetNames(typeof(ChatIntent));

    public GeminiIntentClassifier(IGeminiService geminiService)
    {
        _geminiService = geminiService;
    }

    public async Task<ChatIntent> ClassifyAsync(string question, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(question))
        {
            return ChatIntent.Unknown;
        }

        var prompt = new StringBuilder();
        prompt.AppendLine("Aşağıda StockApp uygulaması için desteklenen intent isimleri yer almaktadır.");
        prompt.AppendLine("Görevin: Kullanıcı sorusunu analiz et ve listede verilen intentlerden EN UYGUN olanının adını tek satırda döndür.");
        prompt.AppendLine("Cevabın sadece intent adını içermeli. Ek açıklama, noktalama veya başka metin ekleme.");
        prompt.AppendLine("Eğer uygun intent yoksa 'Unknown' yaz.");
        prompt.AppendLine();
        prompt.AppendLine("Intent listesi:");
        foreach (var name in IntentNames)
        {
            prompt.AppendLine($"- {name}");
        }
        prompt.AppendLine();
        prompt.AppendLine($"Kullanıcının sorusu: \"{question}\"");

        var response = await _geminiService.GenerateTextAsync(prompt.ToString(), cancellationToken);
        if (!response.Success)
        {
            return ChatIntent.Unknown;
        }

        var text = response.Message?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(text))
        {
            return ChatIntent.Unknown;
        }

        var normalized = text
            .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
            .FirstOrDefault()?
            .Trim()
            .Trim('.', ' ', '"', '\'');

        if (string.IsNullOrWhiteSpace(normalized))
        {
            return ChatIntent.Unknown;
        }

        if (Enum.TryParse<ChatIntent>(normalized, ignoreCase: true, out var parsedIntent))
        {
            return parsedIntent;
        }

        return ChatIntent.Unknown;
    }
}


