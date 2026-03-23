namespace StockApp.Common.Models;

public record PaginationQuery
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;

    public PaginationQuery()
    {
        PageNumber = PageNumber < 1 ? 1 : PageNumber;
        PageSize = PageSize < 1 ? 10 : PageSize;
        PageSize = PageSize > 100 ? 100 : PageSize; // Max 100 items per page
    }
}

