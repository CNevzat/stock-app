using MediatR;
using Microsoft.EntityFrameworkCore;
using StockApp.Entities;
using StockApp.Services;
using StockApp.Common.Constants;
using System.Globalization;
using System.Linq;

namespace StockApp.App.Dashboard.Query;

public record GetDashboardStatsQuery : IRequest<DashboardStatsDto>;

public record DashboardStatsDto
{
    public int TotalCategories { get; init; }
    public int TotalProducts { get; init; }
    public int TotalProductAttributes { get; init; }
    public int TotalStockQuantity { get; init; }
    public int LowStockProducts { get; init; }
    public int OutOfStockProducts { get; init; }
    public int TotalStockMovements { get; init; }
    public int TodayStockIn { get; init; }
    public int TodayStockOut { get; init; }
    public int ThisWeekStockIn { get; init; }
    public int ThisWeekStockOut { get; init; }
    public decimal TotalInventoryCost { get; init; }
    public decimal TotalInventoryPotentialRevenue { get; init; }
    public decimal TotalExpectedSalesRevenue { get; init; }
    public decimal TotalPotentialProfit { get; init; }
    public decimal TotalPurchaseSpent { get; init; }
    public decimal AverageMarginPercentage { get; init; }
    public List<CategoryStatsDto> CategoryStats { get; init; } = new();
    public List<ProductStockDto> ProductStockStatus { get; init; } = new();
    public List<StockDistributionDto> StockDistribution { get; init; } = new();
    public List<CategoryValueDto> CategoryValueDistribution { get; init; } = new();
    public List<ProductValueDto> TopValuableProducts { get; init; } = new();
    public List<RecentStockMovementDto> RecentStockMovements { get; init; } = new();
    public List<StockMovementTrendDto> StockMovementTrend { get; init; } = new(); // Son 1 ay (günlük)
    public List<StockMovementTrendDto> LastYearStockMovementTrend { get; init; } = new(); // Son 1 yıl (aylık)
    public List<MostActiveProductDto> MostActiveProducts { get; init; } = new();
}

public record CategoryStatsDto
{
    public int CategoryId { get; init; }
    public string CategoryName { get; init; } = string.Empty;
    public int ProductCount { get; init; }
    public int TotalStock { get; init; }
}

public record ProductStockDto
{
    public int ProductId { get; init; }
    public string ProductName { get; init; } = string.Empty;
    public string StockCode { get; init; } = string.Empty;
    public int StockQuantity { get; init; }
    public string CategoryName { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty; // "In Stock", "Low Stock", "Out of Stock"
}

public record StockDistributionDto
{
    public string Status { get; init; } = string.Empty;
    public int Count { get; init; }
    public int Percentage { get; init; }
}

public record CategoryValueDto
{
    public int CategoryId { get; init; }
    public string CategoryName { get; init; } = string.Empty;
    public decimal TotalCost { get; init; }
    public decimal TotalPotentialRevenue { get; init; }
    public decimal TotalPotentialProfit { get; init; }
}

public record ProductValueDto
{
    public int ProductId { get; init; }
    public string ProductName { get; init; } = string.Empty;
    public string StockCode { get; init; } = string.Empty;
    public decimal InventoryCost { get; init; }
    public decimal InventoryPotentialRevenue { get; init; }
    public decimal PotentialProfit { get; init; }
}

public record RecentStockMovementDto
{
    public int Id { get; init; }
    public string ProductName { get; init; } = string.Empty;
    public string CategoryName { get; init; } = string.Empty;
    public StockMovementType Type { get; init; }
    public string TypeText => Type == StockMovementType.In ? "Giriş" : "Çıkış";
    public int Quantity { get; init; }
    public string? Description { get; init; }
    public DateTime CreatedAt { get; init; }
}

public record StockMovementTrendDto
{
    public DateTime Date { get; init; }
    public string DateLabel { get; init; } = string.Empty;
    public int StockIn { get; init; }
    public int StockOut { get; init; }
}

public record MostActiveProductDto
{
    public int ProductId { get; init; }
    public string ProductName { get; init; } = string.Empty;
    public string StockCode { get; init; } = string.Empty;
    public string CategoryName { get; init; } = string.Empty;
    public int TotalStockIn { get; init; }
    public int TotalStockOut { get; init; }
    public int NetChange { get; init; }
    public int TotalMovements { get; init; }
}

internal class GetDashboardStatsQueryHandler : IRequestHandler<GetDashboardStatsQuery, DashboardStatsDto>
{
    private readonly ApplicationDbContext _context;
    private readonly ICacheService _cacheService;

    public GetDashboardStatsQueryHandler(ApplicationDbContext context, ICacheService cacheService)
    {
        _context = context;
        _cacheService = cacheService;
    }

    public async Task<DashboardStatsDto> Handle(GetDashboardStatsQuery request, CancellationToken cancellationToken)
    {
        // Önce cache'den kontrol et
        var cachedStats = await _cacheService.GetAsync<DashboardStatsDto>(CacheKeys.DashboardStats, cancellationToken);
        if (cachedStats != null)
        {
            return cachedStats; // Cache'den dön, veritabanına gitme
        }

        // Cache'de yok, veritabanından hesapla
        // Temel istatistikler
        var totalCategories = await _context.Categories.CountAsync(cancellationToken);
        var totalProducts = await _context.Products.CountAsync(cancellationToken);
        var totalProductAttributes = await _context.ProductAttributes.CountAsync(cancellationToken);
        var totalStockQuantity = await _context.Products.SumAsync(p => p.StockQuantity, cancellationToken);
        // Her ürünün kendi LowStockThreshold değerine göre kontrol
        var lowStockProducts = await _context.Products.CountAsync(p => p.StockQuantity > 0 && p.StockQuantity < p.LowStockThreshold, cancellationToken);
        var outOfStockProducts = await _context.Products.CountAsync(p => p.StockQuantity == 0, cancellationToken);

        var pricingSnapshot = await _context.Products
            .Include(p => p.Category)
            .Select(p => new
            {
                p.Id,
                p.Name,
                p.StockCode,
                p.CategoryId,
                CategoryName = p.Category.Name,
                p.StockQuantity,
                p.CurrentPurchasePrice,
                p.CurrentSalePrice
            })
            .ToListAsync(cancellationToken);

        var totalInventoryCost = pricingSnapshot.Sum(p => p.StockQuantity * p.CurrentPurchasePrice);
        var totalInventoryRevenue = pricingSnapshot.Sum(p => p.StockQuantity * p.CurrentSalePrice);
        var totalPotentialProfit = totalInventoryRevenue - totalInventoryCost;
        var totalPurchaseSpent = await _context.StockMovements
            .Where(sm => sm.Type == StockMovementType.In)
            .SumAsync(sm => (decimal?)sm.UnitPrice * sm.Quantity, cancellationToken) ?? 0m;

        var marginData = pricingSnapshot
            .Where(p => p.CurrentPurchasePrice > 0 && p.CurrentSalePrice > 0)
            .Select(p => ((p.CurrentSalePrice - p.CurrentPurchasePrice) / p.CurrentPurchasePrice) * 100m)
            .ToList();
        var averageMarginPercentage = marginData.Any() ? marginData.Average() : 0m;

        var categoryValueDistribution = pricingSnapshot
            .GroupBy(p => new { p.CategoryId, p.CategoryName })
            .Select(g => new CategoryValueDto
            {
                CategoryId = g.Key.CategoryId,
                CategoryName = g.Key.CategoryName,
                TotalCost = g.Sum(x => x.StockQuantity * x.CurrentPurchasePrice),
                TotalPotentialRevenue = g.Sum(x => x.StockQuantity * x.CurrentSalePrice),
                TotalPotentialProfit = g.Sum(x => x.StockQuantity * (x.CurrentSalePrice - x.CurrentPurchasePrice))
            })
            .OrderByDescending(c => c.TotalPotentialRevenue)
            .ToList();

        var topValuableProducts = pricingSnapshot
            .Select(p => new ProductValueDto
            {
                ProductId = p.Id,
                ProductName = p.Name,
                StockCode = p.StockCode,
                InventoryCost = p.StockQuantity * p.CurrentPurchasePrice,
                InventoryPotentialRevenue = p.StockQuantity * p.CurrentSalePrice,
                PotentialProfit = p.StockQuantity * (p.CurrentSalePrice - p.CurrentPurchasePrice)
            })
            .OrderByDescending(p => p.InventoryPotentialRevenue)
            .Take(10)
            .ToList();

        // Kategori bazlı istatistikler - Top 7 kategori + Diğer
        var allCategoryStats = await _context.Categories
            .Select(c => new CategoryStatsDto
            {
                CategoryId = c.Id,
                CategoryName = c.Name,
                ProductCount = c.Products.Count,
                TotalStock = c.Products.Sum(p => p.StockQuantity)
            })
            .OrderByDescending(c => c.ProductCount)
            .ToListAsync(cancellationToken);

        // En çok ürünü olan ilk 7 kategoriyi al
        var topCategories = allCategoryStats.Take(7).ToList();
        
        // Kalan kategorileri "Diğer" kategorisinde birleştir
        var remainingCategories = allCategoryStats.Skip(7).ToList();
        
        var categoryStats = new List<CategoryStatsDto>(topCategories);
        if (remainingCategories.Any())
        {
            categoryStats.Add(new CategoryStatsDto
            {
                CategoryId = 0, // "Diğer" için özel ID
                CategoryName = "Diğer",
                ProductCount = remainingCategories.Sum(c => c.ProductCount),
                TotalStock = remainingCategories.Sum(c => c.TotalStock)
            });
        }

        // Ürün stok durumu (kritik stoklu ürünler: stokta yok ve düşük stok)
        var productStockStatus = await _context.Products
            .Include(p => p.Category)
            .Where(p => p.StockQuantity < p.LowStockThreshold) // Sadece kritik ürünler
            .OrderBy(p => p.StockQuantity == 0 ? 0 : 1) // Önce stokta yok (0), sonra düşük stok (1)
            .ThenBy(p => p.StockQuantity) // Aynı grup içinde stok miktarına göre
            .Take(10)
            .Select(p => new ProductStockDto
            {
                ProductId = p.Id,
                ProductName = p.Name,
                StockCode = p.StockCode,
                StockQuantity = p.StockQuantity,
                CategoryName = p.Category.Name,
                // Her ürünün kendi LowStockThreshold değerine göre status belirleme
                Status = p.StockQuantity == 0 ? "Stokta Yok" : 
                         p.StockQuantity < p.LowStockThreshold ? "Düşük Stok" : 
                         "Stokta Var"
            })
            .ToListAsync(cancellationToken);

        // Stok dağılımı
        // Her ürünün kendi LowStockThreshold değerine göre inStock kontrolü
        var inStockCount = await _context.Products.CountAsync(p => p.StockQuantity >= p.LowStockThreshold, cancellationToken);
        var lowStockCount = lowStockProducts;
        var outOfStockCount = outOfStockProducts;

        var stockDistribution = new List<StockDistributionDto>();
        if (totalProducts > 0)
        {
            stockDistribution = new List<StockDistributionDto>
            {
                new StockDistributionDto
                {
                    Status = "Stokta Var",
                    Count = inStockCount,
                    Percentage = (int)((inStockCount / (double)totalProducts) * 100)
                },
                new StockDistributionDto
                {
                    Status = "Düşük Stok",
                    Count = lowStockCount,
                    Percentage = (int)((lowStockCount / (double)totalProducts) * 100)
                },
                new StockDistributionDto
                {
                    Status = "Stokta Yok",
                    Count = outOfStockCount,
                    Percentage = (int)((outOfStockCount / (double)totalProducts) * 100)
                }
            };
        }

        // Son 10 stok hareketi
        var recentStockMovements = await _context.StockMovements
            .Include(sm => sm.Product)
            .Include(sm => sm.Category)
            .OrderByDescending(sm => sm.CreatedAt)
            .Take(10)
            .Select(sm => new RecentStockMovementDto
            {
                Id = sm.Id,
                ProductName = sm.Product.Name,
                CategoryName = sm.Category.Name,
                Type = sm.Type,
                Quantity = sm.Quantity,
                Description = sm.Description,
                CreatedAt = sm.CreatedAt
            })
            .ToListAsync(cancellationToken);

        // Stok hareketleri istatistikleri
        var today = DateTime.Today;
        var thisWeekStart = today.AddDays(-(int)today.DayOfWeek);

        var totalStockMovements = await _context.StockMovements.CountAsync(cancellationToken);

        // Bugün toplam giriş/çıkış
        var todayStockIn = await _context.StockMovements
            .Where(sm => sm.CreatedAt >= today && sm.Type == StockMovementType.In)
            .SumAsync(sm => (int?)sm.Quantity, cancellationToken) ?? 0;
        var todayStockOut = await _context.StockMovements
            .Where(sm => sm.CreatedAt >= today && sm.Type == StockMovementType.Out)
            .SumAsync(sm => (int?)sm.Quantity, cancellationToken) ?? 0;

        // Bu hafta toplam giriş/çıkış
        var thisWeekStockIn = await _context.StockMovements
            .Where(sm => sm.CreatedAt >= thisWeekStart && sm.Type == StockMovementType.In)
            .SumAsync(sm => (int?)sm.Quantity, cancellationToken) ?? 0;
        var thisWeekStockOut = await _context.StockMovements
            .Where(sm => sm.CreatedAt >= thisWeekStart && sm.Type == StockMovementType.Out)
            .SumAsync(sm => (int?)sm.Quantity, cancellationToken) ?? 0;

        // Son 1 ay (30 gün) trend (günlük giriş/çıkış) - Sol grafik için
        var last30DaysStart = today.AddDays(-29);
        var monthlyTrendData = await _context.StockMovements
            .Where(sm => sm.CreatedAt >= last30DaysStart)
            .GroupBy(sm => sm.CreatedAt.Date)
            .Select(g => new
            {
                Date = g.Key,
                StockIn = g.Where(sm => sm.Type == StockMovementType.In).Sum(sm => sm.Quantity),
                StockOut = g.Where(sm => sm.Type == StockMovementType.Out).Sum(sm => sm.Quantity)
            })
            .ToListAsync(cancellationToken);

        // Eksik günleri doldur (son 30 gün için)
        var stockMovementTrend = new List<StockMovementTrendDto>();
        for (int i = 29; i >= 0; i--)
        {
            var date = today.AddDays(-i);
            var dayData = monthlyTrendData.FirstOrDefault(t => t.Date.Date == date.Date);
            stockMovementTrend.Add(new StockMovementTrendDto
            {
                Date = date,
                DateLabel = date.ToString("dd MMM"),
                StockIn = dayData?.StockIn ?? 0,
                StockOut = dayData?.StockOut ?? 0
            });
        }

        // Son 1 yıl trend (aylık bazda) - Sağ grafik için
        var last12MonthsStart = today.AddMonths(-11);
        var last12MonthsStartDate = new DateTime(last12MonthsStart.Year, last12MonthsStart.Month, 1);
        var currentMonthStart = new DateTime(today.Year, today.Month, 1);

        var last12MonthsData = await _context.StockMovements
            .Where(sm => sm.CreatedAt >= last12MonthsStartDate)
            .GroupBy(sm => new { sm.CreatedAt.Year, sm.CreatedAt.Month })
            .Select(g => new
            {
                Year = g.Key.Year,
                Month = g.Key.Month,
                StockIn = g.Where(sm => sm.Type == StockMovementType.In).Sum(sm => sm.Quantity),
                StockOut = g.Where(sm => sm.Type == StockMovementType.Out).Sum(sm => sm.Quantity)
            })
            .OrderBy(x => x.Year).ThenBy(x => x.Month)
            .ToListAsync(cancellationToken);

        // Son 12 ayı doldur
        var lastYearTrend = new List<StockMovementTrendDto>();
        var currentDate = last12MonthsStartDate;
        while (currentDate <= currentMonthStart)
        {
            var monthData = last12MonthsData.FirstOrDefault(m => m.Year == currentDate.Year && m.Month == currentDate.Month);
            lastYearTrend.Add(new StockMovementTrendDto
            {
                Date = currentDate,
                DateLabel = currentDate.ToString("MMM yyyy", CultureInfo.GetCultureInfo("tr-TR")),
                StockIn = monthData?.StockIn ?? 0,
                StockOut = monthData?.StockOut ?? 0
            });
            currentDate = currentDate.AddMonths(1);
        }

        // En çok hareket eden ürünler (Top 5)
        var mostActiveProducts = await _context.StockMovements
            .Include(sm => sm.Product)
                .ThenInclude(p => p.Category)
            .GroupBy(sm => new { sm.ProductId, ProductName = sm.Product.Name, StockCode = sm.Product.StockCode, CategoryName = sm.Product.Category.Name })
            .Select(g => new MostActiveProductDto
            {
                ProductId = g.Key.ProductId,
                ProductName = g.Key.ProductName,
                StockCode = g.Key.StockCode,
                CategoryName = g.Key.CategoryName,
                TotalStockIn = g.Where(sm => sm.Type == StockMovementType.In).Sum(sm => sm.Quantity),
                TotalStockOut = g.Where(sm => sm.Type == StockMovementType.Out).Sum(sm => sm.Quantity),
                NetChange = g.Where(sm => sm.Type == StockMovementType.In).Sum(sm => sm.Quantity) - g.Where(sm => sm.Type == StockMovementType.Out).Sum(sm => sm.Quantity),
                TotalMovements = g.Count()
            })
            .OrderByDescending(m => m.TotalMovements)
            .Take(5)
            .ToListAsync(cancellationToken);

        var stats = new DashboardStatsDto
        {
            TotalCategories = totalCategories,
            TotalProducts = totalProducts,
            TotalProductAttributes = totalProductAttributes,
            TotalStockQuantity = totalStockQuantity,
            LowStockProducts = lowStockProducts,
            OutOfStockProducts = outOfStockProducts,
            TotalStockMovements = totalStockMovements,
            TodayStockIn = todayStockIn,
            TodayStockOut = todayStockOut,
            ThisWeekStockIn = thisWeekStockIn,
            ThisWeekStockOut = thisWeekStockOut,
            TotalInventoryCost = totalInventoryCost,
            TotalInventoryPotentialRevenue = totalInventoryRevenue,
            TotalExpectedSalesRevenue = totalInventoryRevenue,
            TotalPotentialProfit = totalPotentialProfit,
            TotalPurchaseSpent = totalPurchaseSpent,
            AverageMarginPercentage = averageMarginPercentage,
            CategoryStats = categoryStats,
            ProductStockStatus = productStockStatus,
            StockDistribution = stockDistribution,
            CategoryValueDistribution = categoryValueDistribution,
            TopValuableProducts = topValuableProducts,
            RecentStockMovements = recentStockMovements,
            StockMovementTrend = stockMovementTrend,
            LastYearStockMovementTrend = lastYearTrend,
            MostActiveProducts = mostActiveProducts
        };

        // Cache'e yaz (60 saniye TTL)
        await _cacheService.SetAsync(CacheKeys.DashboardStats, stats, TimeSpan.FromSeconds(60), cancellationToken);

        return stats;
    }
}

