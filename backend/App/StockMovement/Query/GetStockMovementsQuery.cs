using MediatR;
using Microsoft.Extensions.Logging;
using StockApp.Common.Models;
using StockApp.Common.Constants;
using StockApp.Entities;
using StockApp.Services;

namespace StockApp.App.StockMovement.Query;

public record GetStockMovementsQuery : IRequest<PaginatedList<StockMovementDto>>
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    public int? ProductId { get; init; }
    public int? CategoryId { get; init; }
    public StockMovementType? Type { get; init; }
    public string? SearchTerm { get; init; }
    public DateTime? StartDate { get; init; }
    public DateTime? EndDate { get; init; }
}

public record StockMovementDto
{
    public int Id { get; init; }
    public int ProductId { get; init; }
    public string ProductName { get; init; } = string.Empty;
    public int CategoryId { get; init; }
    public string CategoryName { get; init; } = string.Empty;
    public StockMovementType Type { get; init; }
    public string TypeText => Type == StockMovementType.In ? "Giriş" : "Çıkış";
    public int Quantity { get; init; }
    public decimal UnitPrice { get; init; }
    public decimal TotalValue { get; init; }
    public string? Description { get; init; }
    public DateTime CreatedAt { get; init; }
    public int CurrentStockQuantity { get; init; }
    public int LowStockThreshold { get; init; }
}

internal class GetStockMovementsQueryHandler : IRequestHandler<GetStockMovementsQuery, PaginatedList<StockMovementDto>>
{
    private readonly IElasticsearchService _elasticsearchService;
    private readonly ICacheService _cacheService;
    private readonly ILogger<GetStockMovementsQueryHandler> _logger;

    public GetStockMovementsQueryHandler(
        ICacheService cacheService,
        ILogger<GetStockMovementsQueryHandler> logger,
        IElasticsearchService elasticsearchService)
    {
        _cacheService = cacheService;
        _logger = logger;
        _elasticsearchService = elasticsearchService;
    }

    public async Task<PaginatedList<StockMovementDto>> Handle(GetStockMovementsQuery request, CancellationToken cancellationToken)
    {
        // Cache key oluştur
        var cacheKey = CacheKeys.StockMovementsList(
            request.PageNumber,
            request.PageSize,
            request.SearchTerm,
            request.StartDate,
            request.EndDate);

        // Cache'den oku
        var cachedResult = await _cacheService.GetAsync<PaginatedList<StockMovementDto>>(cacheKey, cancellationToken);
        if (cachedResult != null)
        {
            return cachedResult;
        }

        // Elasticsearch zorunlu - tüm aramalar Elasticsearch üzerinden yapılır
        _logger.LogInformation("Using Elasticsearch for stock movement search. SearchTerm: {SearchTerm}, ProductId: {ProductId}, CategoryId: {CategoryId}, Type: {Type}, StartDate: {StartDate}, EndDate: {EndDate}", 
            request.SearchTerm ?? "(all)", request.ProductId, request.CategoryId, request.Type, request.StartDate, request.EndDate);

        var searchResult = await _elasticsearchService.SearchStockMovementsAsync(
            request.SearchTerm ?? string.Empty, // Empty string = MatchAll
            request.PageNumber,
            request.PageSize,
            request.ProductId,
            request.CategoryId,
            request.Type,
            request.StartDate,
            request.EndDate,
            cancellationToken);

        var result = new PaginatedList<StockMovementDto>(
            searchResult.Items,
            (int)searchResult.TotalCount,
            searchResult.Page,
            searchResult.PageSize);
        
        _logger.LogInformation("Elasticsearch returned {Count} results", searchResult.Items.Count);

        // Cache'e yaz (60 saniye TTL)
        await _cacheService.SetAsync(cacheKey, result, TimeSpan.FromSeconds(60), cancellationToken);

        return result;
    }
}


