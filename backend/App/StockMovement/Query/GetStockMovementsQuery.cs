using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using StockApp;
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
    private readonly ApplicationDbContext _context;
    private readonly IElasticsearchService _elasticsearchService;
    private readonly ICacheService _cacheService;
    private readonly ILogger<GetStockMovementsQueryHandler> _logger;

    public GetStockMovementsQueryHandler(
        ApplicationDbContext context,
        ICacheService cacheService,
        ILogger<GetStockMovementsQueryHandler> logger,
        IElasticsearchService elasticsearchService)
    {
        _context = context;
        _cacheService = cacheService;
        _logger = logger;
        _elasticsearchService = elasticsearchService;
    }

    public async Task<PaginatedList<StockMovementDto>> Handle(GetStockMovementsQuery request, CancellationToken cancellationToken)
    {
        var generation = await _cacheService.GetStockMovementsListGenerationAsync(cancellationToken);
        var cacheKey = CacheKeys.StockMovementsList(
            request.PageNumber,
            request.PageSize,
            request.SearchTerm,
            request.StartDate,
            request.EndDate,
            request.ProductId,
            request.CategoryId,
            request.Type,
            generation);

        // Cache'den oku
        var cachedResult = await _cacheService.GetAsync<PaginatedList<StockMovementDto>>(cacheKey, cancellationToken);
        if (cachedResult != null)
        {
            return cachedResult;
        }

        // Ürün detayı: ProductId + metin araması yok — kaynak PostgreSQL (ES indeks gecikmesi / tutarsızlığı olmaz)
        if (request.ProductId.HasValue && string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            _logger.LogInformation(
                "Using database for stock movements by product. ProductId: {ProductId}, CategoryId: {CategoryId}, Type: {Type}",
                request.ProductId, request.CategoryId, request.Type);

            var fromDb = await GetStockMovementsFromDatabaseAsync(request, cancellationToken);
            await _cacheService.SetAsync(cacheKey, fromDb, TimeSpan.FromSeconds(60), cancellationToken);
            return fromDb;
        }

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

    private async Task<PaginatedList<StockMovementDto>> GetStockMovementsFromDatabaseAsync(
        GetStockMovementsQuery request,
        CancellationToken cancellationToken)
    {
        var query = _context.StockMovements
            .AsNoTracking()
            .Where(sm => sm.ProductId == request.ProductId!.Value);

        if (request.CategoryId.HasValue)
            query = query.Where(sm => sm.CategoryId == request.CategoryId.Value);
        if (request.Type.HasValue)
            query = query.Where(sm => sm.Type == request.Type.Value);
        if (request.StartDate.HasValue)
            query = query.Where(sm => sm.CreatedAt >= request.StartDate.Value);
        if (request.EndDate.HasValue)
        {
            var endInclusive = request.EndDate.Value.Date.AddDays(1).AddTicks(-1);
            query = query.Where(sm => sm.CreatedAt <= endInclusive);
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(sm => sm.CreatedAt)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(sm => new StockMovementDto
            {
                Id = sm.Id,
                ProductId = sm.ProductId,
                ProductName = sm.Product.Name,
                CategoryId = sm.CategoryId,
                CategoryName = sm.Category.Name,
                Type = sm.Type,
                Quantity = sm.Quantity,
                UnitPrice = sm.UnitPrice,
                TotalValue = sm.UnitPrice * sm.Quantity,
                Description = sm.Description,
                CreatedAt = sm.CreatedAt,
                CurrentStockQuantity = sm.Product.StockQuantity,
                LowStockThreshold = sm.Product.LowStockThreshold
            })
            .ToListAsync(cancellationToken);

        return new PaginatedList<StockMovementDto>(items, totalCount, request.PageNumber, request.PageSize);
    }
}


