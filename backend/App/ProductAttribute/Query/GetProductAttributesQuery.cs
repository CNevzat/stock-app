using MediatR;
using Microsoft.EntityFrameworkCore;
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
    private readonly ApplicationDbContext _context;

    public GetProductAttributesQueryHandler(
        ICacheService cacheService,
        ILogger<GetProductAttributesQueryHandler> logger,
        IElasticsearchService elasticsearchService,
        ApplicationDbContext context)
    {
        _cacheService = cacheService;
        _logger = logger;
        _elasticsearchService = elasticsearchService;
        _context = context;
    }

    public async Task<PaginatedList<ProductAttributeDto>> Handle(GetProductAttributesQuery request, CancellationToken cancellationToken)
    {
        // Cache key oluştur
        var cacheKey = CacheKeys.ProductAttributesList(
            request.PageNumber,
            request.PageSize,
            request.ProductId,
            request.SearchKey);

        var cachedResult = await _cacheService.GetAsync<PaginatedList<ProductAttributeDto>>(cacheKey, cancellationToken);
        if (cachedResult != null &&
            !(cachedResult.TotalCount == 0 &&
              string.IsNullOrWhiteSpace(request.SearchKey) &&
              !request.ProductId.HasValue))
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

        PaginatedList<ProductAttributeDto> result;

        if (searchResult.TotalCount == 0 && string.IsNullOrWhiteSpace(request.SearchKey))
        {
            _logger.LogWarning("Elasticsearch öznitelik listesi boş; SQLite üzerinden okunuyor.");
            result = await GetProductAttributesFromDatabaseAsync(request, cancellationToken);
        }
        else
        {
            result = new PaginatedList<ProductAttributeDto>(
                searchResult.Items,
                (int)searchResult.TotalCount,
                searchResult.Page,
                searchResult.PageSize);
        }

        _logger.LogInformation("Öznitelik listesi satır sayısı: {Count}", result.Items.Count);

        if (result.TotalCount > 0)
        {
            await _cacheService.SetAsync(cacheKey, result, TimeSpan.FromSeconds(60), cancellationToken);
        }

        return result;
    }

    private async Task<PaginatedList<ProductAttributeDto>> GetProductAttributesFromDatabaseAsync(
        GetProductAttributesQuery request,
        CancellationToken cancellationToken)
    {
        var query = _context.ProductAttributes.AsNoTracking().Include(pa => pa.Product).AsQueryable();

        if (request.ProductId.HasValue)
            query = query.Where(pa => pa.ProductId == request.ProductId.Value);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderBy(pa => pa.Product.Name)
            .ThenBy(pa => pa.Key)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(pa => new ProductAttributeDto
            {
                Id = pa.Id,
                ProductId = pa.ProductId,
                ProductName = pa.Product.Name,
                Key = pa.Key,
                Value = pa.Value,
                CreatedAt = pa.CreatedAt,
                UpdatedAt = pa.UpdatedAt
            })
            .ToListAsync(cancellationToken);

        return new PaginatedList<ProductAttributeDto>(items, totalCount, request.PageNumber, request.PageSize);
    }
}

