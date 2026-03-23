using MediatR;
using Microsoft.EntityFrameworkCore;
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
    private readonly ApplicationDbContext _context;

    public GetProductsQueryHandler(
        ICacheService cacheService,
        ILogger<GetProductsQueryHandler> logger,
        IElasticsearchService elasticsearchService,
        ApplicationDbContext context)
    {
        _cacheService = cacheService;
        _logger = logger;
        _elasticsearchService = elasticsearchService;
        _context = context;
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

        // Cache'den oku (Redis boş sonucu önbelleğe aldıysa, ES henüz doldurulmadan kalabilir — boş cache'i atla)
        var cachedResult = await _cacheService.GetAsync<PaginatedList<ProductDto>>(cacheKey, cancellationToken);
        if (cachedResult != null &&
            !(cachedResult.TotalCount == 0 &&
              string.IsNullOrWhiteSpace(request.SearchTerm) &&
              !request.CategoryId.HasValue &&
              !request.LocationId.HasValue))
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

        PaginatedList<ProductDto> result;

        // Dashboard DB'den sayar; liste ES'ten gelir. İndeks boş / reindex yapılmadıysa burada DB'den doldur.
        if (searchResult.TotalCount == 0 && string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            _logger.LogWarning("Elasticsearch ürün listesi boş; SQLite üzerinden okunuyor.");
            result = await GetProductsFromDatabaseAsync(request, cancellationToken);
        }
        else
        {
            result = new PaginatedList<ProductDto>(
                searchResult.Items,
                (int)searchResult.TotalCount,
                searchResult.Page,
                searchResult.PageSize);
        }

        _logger.LogInformation("Ürün listesi satır sayısı: {Count}", result.Items.Count);

        if (result.TotalCount > 0)
        {
            await _cacheService.SetAsync(cacheKey, result, TimeSpan.FromSeconds(60), cancellationToken);
        }

        return result;
    }

    private async Task<PaginatedList<ProductDto>> GetProductsFromDatabaseAsync(
        GetProductsQuery request,
        CancellationToken cancellationToken)
    {
        var query = _context.Products
            .AsNoTracking()
            .Include(p => p.Category)
            .Include(p => p.Location)
            .AsQueryable();

        if (request.CategoryId.HasValue)
            query = query.Where(p => p.CategoryId == request.CategoryId.Value);
        if (request.LocationId.HasValue)
            query = query.Where(p => p.LocationId == request.LocationId.Value);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderBy(p => p.Name)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(p => new ProductDto
            {
                Id = p.Id,
                Name = p.Name,
                StockCode = p.StockCode,
                Description = p.Description,
                StockQuantity = p.StockQuantity,
                LowStockThreshold = p.LowStockThreshold,
                CategoryId = p.CategoryId,
                CategoryName = p.Category.Name,
                LocationId = p.LocationId,
                LocationName = p.Location != null ? p.Location.Name : null,
                ImagePath = p.ImagePath,
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt,
                CurrentPurchasePrice = p.CurrentPurchasePrice,
                CurrentSalePrice = p.CurrentSalePrice
            })
            .ToListAsync(cancellationToken);

        return new PaginatedList<ProductDto>(items, totalCount, request.PageNumber, request.PageSize);
    }
}

