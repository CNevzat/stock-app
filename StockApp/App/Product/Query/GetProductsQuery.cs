using MediatR;
using Microsoft.Extensions.Logging;
using StockApp.Common.Models;
using StockApp.Common.Constants;
using StockApp.Services;

namespace StockApp.App.Product.Query;

public record GetProductsQuery : IRequest<PaginatedList<ProductDto>>
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    public int? CategoryId { get; init; }
    public int? LocationId { get; init; }
    public string? SearchTerm { get; init; }
}

public record ProductDto
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string StockCode { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public int StockQuantity { get; init; }
    public int LowStockThreshold { get; init; }
    public int CategoryId { get; init; }
    public string CategoryName { get; init; } = string.Empty;
    public int? LocationId { get; init; }
    public string? LocationName { get; init; }
    public string? ImagePath { get; init; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public decimal CurrentPurchasePrice { get; init; }
    public decimal CurrentSalePrice { get; init; }
}

internal class GetProductsQueryHandler : IRequestHandler<GetProductsQuery, PaginatedList<ProductDto>>
{
    private readonly IElasticsearchService _elasticsearchService;
    private readonly ICacheService _cacheService;
    private readonly ILogger<GetProductsQueryHandler> _logger;

    public GetProductsQueryHandler(
        ICacheService cacheService,
        ILogger<GetProductsQueryHandler> logger,
        IElasticsearchService elasticsearchService)
    {
        _cacheService = cacheService;
        _logger = logger;
        _elasticsearchService = elasticsearchService;
    }

    public async Task<PaginatedList<ProductDto>> Handle(GetProductsQuery request, CancellationToken cancellationToken)
    {
        // Cache key oluştur
        var cacheKey = CacheKeys.ProductsList(
            request.PageNumber,
            request.PageSize,
            request.CategoryId,
            request.LocationId,
            request.SearchTerm);

        // Cache'den oku
        var cachedResult = await _cacheService.GetAsync<PaginatedList<ProductDto>>(cacheKey, cancellationToken);
        if (cachedResult != null)
        {
            return cachedResult;
        }

        // Elasticsearch zorunlu - tüm aramalar Elasticsearch üzerinden yapılır
        _logger.LogInformation("Using Elasticsearch for product search. SearchTerm: {SearchTerm}, CategoryId: {CategoryId}, LocationId: {LocationId}", 
            request.SearchTerm ?? "(all)", request.CategoryId, request.LocationId);

        var searchResult = await _elasticsearchService.SearchProductsAsync(
            request.SearchTerm ?? string.Empty, // Empty string = MatchAll
            request.PageNumber,
            request.PageSize,
            request.CategoryId,
            request.LocationId,
            cancellationToken);

        var result = new PaginatedList<ProductDto>(
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

