namespace StockApp.Options;

public class GeminiOptions
{
    public const string SectionName = "Gemini";
    
    public string Model { get; set; } = "gemini-2.5-flash";
    
    public string? ApiEndpoint { get; set; } = "https://generativelanguage.googleapis.com/v1";
    
    public string? ApiKey { get; set; }
}


