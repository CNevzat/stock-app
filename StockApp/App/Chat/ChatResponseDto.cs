namespace StockApp.App.Chat;

public record ChatResponseDto(
    string Answer,
    ChatIntent Intent,
    IReadOnlyList<string>? Suggestions = null,
    string? DebugContext = null);



