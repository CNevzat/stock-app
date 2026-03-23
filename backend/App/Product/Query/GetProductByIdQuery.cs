using MediatR;
using Microsoft.EntityFrameworkCore;
using StockApp.Entities;

namespace StockApp.App.Product.Query;

public record GetProductByIdQuery : IRequest<ProductDetailDto?>
{
    public int Id { get; init; }
}

public record ProductDetailDto
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
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
    public List<ProductAttributeDto> Attributes { get; init; } = new();
    public decimal CurrentPurchasePrice { get; init; }
    public decimal CurrentSalePrice { get; init; }
    public decimal AveragePurchasePrice { get; init; }
    public decimal AverageSalePrice { get; init; }
    public List<ProductPriceDto> PriceHistory { get; init; } = new();
}

public record ProductAttributeDto
{
    public int Id { get; init; }
    public string Key { get; init; } = string.Empty;
    public string Value { get; init; } = string.Empty;
}

public record ProductPriceDto
{
    public int Id { get; init; }
    public decimal PurchasePrice { get; init; }
    public decimal SalePrice { get; init; }
    public DateTime EffectiveDate { get; init; }
}

internal class GetProductByIdQueryHandler : IRequestHandler<GetProductByIdQuery, ProductDetailDto?>
{
    private readonly ApplicationDbContext _context;

    public GetProductByIdQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ProductDetailDto?> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
    {
        var product = await _context.Products
            .Include(p => p.Category)
            .Include(p => p.Location)
            .Include(p => p.Attributes)
            .Include(p => p.PriceHistory)
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

        if (product == null)
        {
            return null;
        }

        var stockMovements = await _context.StockMovements
            .Where(sm => sm.ProductId == product.Id)
            .Select(sm => new { sm.Type, sm.Quantity, sm.UnitPrice, sm.CreatedAt })
            .ToListAsync(cancellationToken);

        var purchaseMovements = stockMovements.Where(sm => sm.Type == StockMovementType.In).ToList();
        var totalPurchaseQuantity = purchaseMovements.Sum(sm => (decimal)sm.Quantity);
        var averagePurchasePrice = totalPurchaseQuantity > 0
            ? purchaseMovements.Sum(sm => sm.UnitPrice * sm.Quantity) / totalPurchaseQuantity
            : product.CurrentPurchasePrice;
        var lastPurchasePrice = purchaseMovements
            .OrderByDescending(sm => sm.CreatedAt)
            .FirstOrDefault()?.UnitPrice ?? product.CurrentPurchasePrice;

        var saleMovements = stockMovements.Where(sm => sm.Type == StockMovementType.Out).ToList();
        var totalSaleQuantity = saleMovements.Sum(sm => (decimal)sm.Quantity);
        var averageSalePrice = totalSaleQuantity > 0
            ? saleMovements.Sum(sm => sm.UnitPrice * sm.Quantity) / totalSaleQuantity
            : product.CurrentSalePrice;
        var lastSalePrice = saleMovements
            .OrderByDescending(sm => sm.CreatedAt)
            .FirstOrDefault()?.UnitPrice ?? product.CurrentSalePrice;

        return new ProductDetailDto
        {
            Id = product.Id,
            Name = product.Name,
            StockCode = product.StockCode,
            Description = product.Description,
            StockQuantity = product.StockQuantity,
            LowStockThreshold = product.LowStockThreshold,
            CategoryId = product.CategoryId,
            CategoryName = product.Category.Name,
            LocationId = product.LocationId,
            LocationName = product.Location != null ? product.Location.Name : null,
            ImagePath = product.ImagePath,
            CreatedAt = product.CreatedAt,
            UpdatedAt = product.UpdatedAt,
            CurrentPurchasePrice = lastPurchasePrice,
            CurrentSalePrice = lastSalePrice,
            AveragePurchasePrice = averagePurchasePrice,
            AverageSalePrice = averageSalePrice,
            Attributes = product.Attributes.Select(a => new ProductAttributeDto
            {
                Id = a.Id,
                Key = a.Key,
                Value = a.Value
            }).ToList(),
            PriceHistory = product.PriceHistory
                .OrderByDescending(ph => ph.EffectiveDate)
                .Take(20)
                .Select(ph => new ProductPriceDto
                {
                    Id = ph.Id,
                    PurchasePrice = ph.PurchasePrice,
                    SalePrice = ph.SalePrice,
                    EffectiveDate = ph.EffectiveDate
                })
                .OrderBy(ph => ph.EffectiveDate)
                .ToList()
        };
    }
}

