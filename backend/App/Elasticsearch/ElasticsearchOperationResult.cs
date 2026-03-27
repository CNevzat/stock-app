namespace StockApp.App.Elasticsearch;

// Elasticsearch işlemlerinin controller'a taşınmaması için ortak sonuç tipi.
public sealed record ElasticsearchOperationResult
{
    public bool Success { get; init; }
    public int HttpStatus { get; init; } = 200;
    public object? Data { get; init; }
    public string? ErrorMessage { get; init; }
    public string? ErrorTitle { get; init; }
    public bool UseProblemDetails { get; init; } = true;

    public static ElasticsearchOperationResult Ok(object data) =>
        new() { Success = true, HttpStatus = 200, Data = data };

    public static ElasticsearchOperationResult BadRequest(string message) =>
        new() { Success = false, HttpStatus = 400, ErrorMessage = message, UseProblemDetails = false };

    public static ElasticsearchOperationResult Problem(int statusCode, string title, string detail, bool useProblemDetails = true) =>
        new()
        {
            Success = false,
            HttpStatus = statusCode,
            ErrorTitle = title,
            ErrorMessage = detail,
            UseProblemDetails = useProblemDetails
        };
}
