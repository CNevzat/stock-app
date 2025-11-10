using MediatR;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using StockApp.App.Dashboard.Query;
using StockApp.App.Product.Query;
using StockApp.Entities;
using StockApp.Hub;

namespace StockApp.App.Product.Command;

public record CreateProductCommand : IRequest<CreateProductCommandResponse>
{
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public int StockQuantity { get; init; }
    public int LowStockThreshold { get; init; } = 5; // Default 5
    public int CategoryId { get; init; }
    public int? LocationId { get; init; }
    public decimal PurchasePrice { get; init; }
    public decimal SalePrice { get; init; }
}

public record CreateProductCommandResponse
{
    public int ProductId { get; set; }
}

internal class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, CreateProductCommandResponse>
{
    private readonly ApplicationDbContext _context;
    private readonly IHubContext<StockHub> _hubContext;
    private readonly IMediator _mediator;

    public CreateProductCommandHandler(ApplicationDbContext context, IHubContext<StockHub> hubContext, IMediator mediator)
    {
        _context = context;
        _hubContext = hubContext;
        _mediator = mediator;
    }

    public async Task<CreateProductCommandResponse> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        if (request.PurchasePrice <= 0)
        {
            throw new ArgumentException("Satın alma fiyatı 0'dan büyük olmalıdır.", nameof(request.PurchasePrice));
        }

        if (request.SalePrice <= 0)
        {
            throw new ArgumentException("Satış fiyatı 0'dan büyük olmalıdır.", nameof(request.SalePrice));
        }

        // Generate unique stock code
        var stockCode = await GenerateUniqueStockCodeAsync(cancellationToken);

        var product = new Entities.Product
        {
            Name = request.Name,
            StockCode = stockCode,
            Description = request.Description,
            StockQuantity = request.StockQuantity,
            LowStockThreshold = request.LowStockThreshold,
            CategoryId = request.CategoryId,
            LocationId = request.LocationId,
            CreatedAt = DateTime.UtcNow,
            CurrentPurchasePrice = request.PurchasePrice,
            CurrentSalePrice = request.SalePrice
        };

        _context.Products.Add(product);
        await _context.SaveChangesAsync(cancellationToken);

        // Price history kaydı oluştur
        var priceHistory = new ProductPrice
        {
            ProductId = product.Id,
            PurchasePrice = request.PurchasePrice,
            SalePrice = request.SalePrice,
            EffectiveDate = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };
        _context.ProductPrices.Add(priceHistory);
        await _context.SaveChangesAsync(cancellationToken);

        // İlk stok miktarı varsa otomatik stok hareketi oluştur
        if (request.StockQuantity > 0)
        {
            var initialStockMovement = new Entities.StockMovement
            {
                ProductId = product.Id,
                CategoryId = product.CategoryId,
                Type = StockMovementType.In,
                Quantity = request.StockQuantity,
                UnitPrice = request.PurchasePrice,
                Description = "İlk Giriş",
                CreatedAt = DateTime.UtcNow
            };

            _context.StockMovements.Add(initialStockMovement);
            await _context.SaveChangesAsync(cancellationToken);
        }

        // SignalR ile dashboard stats gönder
        try
        {
            var dashboardStats = await _mediator.Send(new GetDashboardStatsQuery(), cancellationToken);
            await _hubContext.Clients.All.SendAsync("DashboardStatsUpdated", dashboardStats, cancellationToken);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"SignalR gönderim hatası: {ex.Message}");
        }

        // SignalR ile yeni ürünü tüm client'lara gönder
        try
        {
            var productDetail = await _mediator.Send(new GetProductByIdQuery { Id = product.Id }, cancellationToken);
            if (productDetail != null)
            {
                await _hubContext.Clients.All.SendAsync("ProductCreated", productDetail, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"SignalR product created gönderim hatası: {ex.Message}");
        }

        return new CreateProductCommandResponse
        {
            ProductId = product.Id
        };
    }

    private async Task<string> GenerateUniqueStockCodeAsync(CancellationToken cancellationToken)
    {
        const string letters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        const string numbers = "0123456789";
        var random = new Random();
        
        while (true)
        {
            // Generate stock code like "ABC433"
            var stockCode = new string(Enumerable.Range(0, 3)
                .Select(_ => letters[random.Next(letters.Length)])
                .ToArray()) + new string(Enumerable.Range(0, 3)
                .Select(_ => numbers[random.Next(numbers.Length)])
                .ToArray());

            // Check if stock code already exists
            var exists = await _context.Products
                .AnyAsync(p => p.StockCode == stockCode, cancellationToken);

            if (!exists)
            {
                return stockCode;
            }
        }
    }
}

