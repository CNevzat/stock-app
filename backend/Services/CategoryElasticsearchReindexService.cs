using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using StockApp;
using StockApp.App.Product.Query;
using StockApp.App.StockMovement.Query;

namespace StockApp.Services;

public interface ICategoryElasticsearchReindexService
{
    /// <summary>
    /// Kategori adı değişiminden sonra ilgili ürün ve stok hareketi ES dokümanlarını toplu günceller.
    /// Hangfire job olarak çalıştırılmak üzere tasarlanmıştır; sayfalı DB okuma + bulk index ile büyük kategorileri destekler.
    /// </summary>
    Task ReindexAfterCategoryNameChangeAsync(int categoryId);
}

internal sealed class CategoryElasticsearchReindexService : ICategoryElasticsearchReindexService
{
    private const int PageSize = 500;

    private readonly ApplicationDbContext _context;
    private readonly IMediator _mediator;
    private readonly IElasticsearchService _elasticsearchService;
    private readonly ILogger<CategoryElasticsearchReindexService> _logger;

    public CategoryElasticsearchReindexService(
        ApplicationDbContext context,
        IMediator mediator,
        IElasticsearchService elasticsearchService,
        ILogger<CategoryElasticsearchReindexService> logger)
    {
        _context = context;
        _mediator = mediator;
        _elasticsearchService = elasticsearchService;
        _logger = logger;
    }

    public async Task ReindexAfterCategoryNameChangeAsync(int categoryId)
    {
        _logger.LogInformation("ES kategori reindex başladı: CategoryId={CategoryId}", categoryId);

        var totalProducts = await ReindexProductsAsync(categoryId);
        var totalMovements = await ReindexStockMovementsAsync(categoryId);

        _logger.LogInformation(
            "ES kategori reindex tamamlandı: CategoryId={CategoryId}, Ürün={ProductCount}, StokHareketi={MovementCount}",
            categoryId, totalProducts, totalMovements);
    }

    private async Task<int> ReindexProductsAsync(int categoryId)
    {
        var total = 0;
        var page = 0;

        while (true)
        {
            // Sayfalı ID listesi çek — tüm ürünleri bellekte tutma
            var productIds = await _context.Products
                .AsNoTracking()
                .Where(p => p.CategoryId == categoryId)
                .OrderBy(p => p.Id)
                .Skip(page * PageSize)
                .Take(PageSize)
                .Select(p => p.Id)
                .ToListAsync();

            if (productIds.Count == 0)
                break;

            var batch = new List<ProductDto>(productIds.Count);
            foreach (var productId in productIds)
            {
                var detail = await _mediator.Send(new GetProductByIdQuery { Id = productId });
                if (detail == null)
                    continue;

                batch.Add(new ProductDto
                {
                    Id = detail.Id,
                    Name = detail.Name,
                    StockCode = detail.StockCode,
                    Description = detail.Description,
                    StockQuantity = detail.StockQuantity,
                    LowStockThreshold = detail.LowStockThreshold,
                    CategoryId = detail.CategoryId,
                    CategoryName = detail.CategoryName,
                    LocationId = detail.LocationId,
                    LocationName = detail.LocationName,
                    ImagePath = detail.ImagePath,
                    CreatedAt = detail.CreatedAt,
                    UpdatedAt = detail.UpdatedAt,
                    CurrentPurchasePrice = detail.CurrentPurchasePrice,
                    CurrentSalePrice = detail.CurrentSalePrice
                });
            }

            if (batch.Count > 0)
                await _elasticsearchService.BulkIndexProductsAsync(batch);

            total += batch.Count;
            _logger.LogDebug("Ürün reindex sayfası: CategoryId={CategoryId}, Sayfa={Page}, Adet={Count}", categoryId, page + 1, batch.Count);

            if (productIds.Count < PageSize)
                break;

            page++;
        }

        return total;
    }

    private async Task<int> ReindexStockMovementsAsync(int categoryId)
    {
        var total = 0;
        var page = 0;

        while (true)
        {
            var batch = await _context.StockMovements
                .AsNoTracking()
                .Where(sm => sm.CategoryId == categoryId)
                .OrderBy(sm => sm.Id)
                .Skip(page * PageSize)
                .Take(PageSize)
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
                .ToListAsync();

            if (batch.Count == 0)
                break;

            await _elasticsearchService.BulkIndexStockMovementsAsync(batch);

            total += batch.Count;
            _logger.LogDebug("StokHareketi reindex sayfası: CategoryId={CategoryId}, Sayfa={Page}, Adet={Count}", categoryId, page + 1, batch.Count);

            if (batch.Count < PageSize)
                break;

            page++;
        }

        return total;
    }
}
