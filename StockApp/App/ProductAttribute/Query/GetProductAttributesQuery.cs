using MediatR;
using Microsoft.Extensions.Logging;
using StockApp.Common.Models;
using StockApp.Common.Constants;
using StockApp.Services;

namespace StockApp.App.ProductAttribute.Query;

public record GetProductAttributesQuery : IRequest<PaginatedList<ProductAttributeDto>>
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    public int? ProductId { get; init; }
    public string? SearchKey { get; init; }
}

public record ProductAttributeDto
{
    public int Id { get; init; }
    public int ProductId { get; init; }
    public string ProductName { get; init; } = string.Empty;
    public string Key { get; init; } = string.Empty;
    public string Value { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}

internal class GetProductAttributesQueryHandler : IRequestHandler<GetProductAttributesQuery, PaginatedList<ProductAttributeDto>>
{
    private readonly IElasticsearchService _elasticsearchService;
    private readonly ICacheService _cacheService;
    private readonly ILogger<GetProductAttributesQueryHandler> _logger;

    public GetProductAttributesQueryHandler(
        ICacheService cacheService,
        ILogger<GetProductAttributesQueryHandler> logger,
        IElasticsearchService elasticsearchService)
    {
        _cacheService = cacheService;
        _logger = logger;
        _elasticsearchService = elasticsearchService;
    }

    public async Task<PaginatedList<ProductAttributeDto>> Handle(GetProductAttributesQuery request, CancellationToken cancellationToken)
    {
        // Cache key oluştur
        var cacheKey = CacheKeys.ProductAttributesList(
            request.PageNumber,
            request.PageSize,
            request.ProductId,
            request.SearchKey);

        // Cache'den oku
        var cachedResult = await _cacheService.GetAsync<PaginatedList<ProductAttributeDto>>(cacheKey, cancellationToken);
        if (cachedResult != null)
        {
            return cachedResult;
        }

        // Elasticsearch zorunlu - tüm aramalar Elasticsearch üzerinden yapılır
        _logger.LogInformation("Using Elasticsearch for product attribute search. SearchKey: {SearchKey}, ProductId: {ProductId}", 
            request.SearchKey ?? "(all)", request.ProductId);

        var searchResult = await _elasticsearchService.SearchProductAttributesAsync(
            request.SearchKey ?? string.Empty, // Empty string = MatchAll
            request.PageNumber,
            request.PageSize,
            request.ProductId,
            cancellationToken);

        var result = new PaginatedList<ProductAttributeDto>(
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

