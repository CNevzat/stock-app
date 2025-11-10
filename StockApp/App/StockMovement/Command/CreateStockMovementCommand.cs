using MediatR;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using StockApp.App.Dashboard.Query;
using StockApp.Entities;
using StockApp.Hub;
using StockApp.App.Product.Query;

namespace StockApp.App.StockMovement.Command;

public record CreateStockMovementCommand : IRequest<CreateStockMovementCommandResponse>
{
    public int ProductId { get; init; }
    public StockMovementType Type { get; init; }
    public int Quantity { get; init; }
    public decimal UnitPrice { get; init; }
    public string? Description { get; init; }
}

public record CreateStockMovementCommandResponse(int Id);

internal class CreateStockMovementCommandHandler : IRequestHandler<CreateStockMovementCommand, CreateStockMovementCommandResponse>
{
    private readonly ApplicationDbContext _context;
    private readonly IHubContext<StockHub> _hubContext;
    private readonly IMediator _mediator;

    public CreateStockMovementCommandHandler(ApplicationDbContext context, IHubContext<StockHub> hubContext, IMediator mediator)
    {
        _context = context;
        _hubContext = hubContext;
        _mediator = mediator;
    }

    public async Task<CreateStockMovementCommandResponse> Handle(CreateStockMovementCommand request, CancellationToken cancellationToken)
    {
        if (request.UnitPrice <= 0)
        {
            throw new ArgumentException("Birim fiyat 0'dan büyük olmalıdır.", nameof(request.UnitPrice));
        }

        // Ürünü bul
        var product = await _context.Products
            .FirstOrDefaultAsync(p => p.Id == request.ProductId, cancellationToken);

        if (product == null)
        {
            throw new KeyNotFoundException($"Ürün bulunamadı: {request.ProductId}");
        }

        // Çıkış işleminde stok kontrolü
        if (request.Type == StockMovementType.Out)
        {
            if (product.StockQuantity < request.Quantity)
            {
                throw new InvalidOperationException(
                    $"Yetersiz stok! Mevcut stok: {product.StockQuantity}, İstenen miktar: {request.Quantity}");
            }
        }

        // StockMovement kaydı oluştur
        var now = DateTime.UtcNow;
        var stockMovement = new Entities.StockMovement
        {
            ProductId = request.ProductId,
            CategoryId = product.CategoryId,
            Type = request.Type,
            Quantity = request.Quantity,
            UnitPrice = request.UnitPrice,
            Description = request.Description,
            CreatedAt = now
        };

        _context.StockMovements.Add(stockMovement);

        // Ürün stok miktarını güncelle
        if (request.Type == StockMovementType.In)
        {
            product.StockQuantity += request.Quantity;
        }
        else // Out
        {
            product.StockQuantity -= request.Quantity;
        }

        var priceChanged = false;

        if (request.Type == StockMovementType.In && product.CurrentPurchasePrice != request.UnitPrice)
        {
            product.CurrentPurchasePrice = request.UnitPrice;
            priceChanged = true;
        }

        if (request.Type == StockMovementType.Out && product.CurrentSalePrice != request.UnitPrice)
        {
            product.CurrentSalePrice = request.UnitPrice;
            priceChanged = true;
        }

        if (priceChanged)
        {
            var priceHistory = new ProductPrice
            {
                ProductId = product.Id,
                PurchasePrice = product.CurrentPurchasePrice,
                SalePrice = product.CurrentSalePrice,
                EffectiveDate = now,
                CreatedAt = now
            };
            _context.ProductPrices.Add(priceHistory);
        }

        product.UpdatedAt = now;

        await _context.SaveChangesAsync(cancellationToken);
        
        // SignalR ile Gerçek zamanlı bildirim gönder
        try
        {
            var dashboardStats = await _mediator.Send(new GetDashboardStatsQuery(), cancellationToken);
            await _hubContext.Clients.All.SendAsync("DashboardStatsUpdated", dashboardStats, cancellationToken);
        }
        catch (Exception e)
        {
            Console.WriteLine("SignalR bildirim gönderilirken hata oluştu: " + e.Message);
        }

        try
        {
            var productDetail = await _mediator.Send(new GetProductByIdQuery { Id = product.Id }, cancellationToken);
            if (productDetail != null)
            {
                await _hubContext.Clients.All.SendAsync("ProductUpdated", productDetail, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"SignalR product updated gönderim hatası: {ex.Message}");
        }

        return new CreateStockMovementCommandResponse(stockMovement.Id);
    }
}


