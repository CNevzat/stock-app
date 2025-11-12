using StockApp.Entities;

namespace StockApp.App.Chat;

public record ChatQuestionContext(
    ChatIntent Intent,
    string OriginalQuestion,
    DateRange? Range = null,
    StockMovementType? MovementType = null,
    string? ProductKeyword = null,
    string? CategoryKeyword = null,
    bool RequiresDetailedList = false);

public record DateRange(DateTime Start, DateTime End)
{
    public static DateRange From(DateTime start, DateTime end) => new(start, end);

    public override string ToString() => $"{Start:dd.MM.yyyy HH:mm} - {End:dd.MM.yyyy HH:mm}";
}


