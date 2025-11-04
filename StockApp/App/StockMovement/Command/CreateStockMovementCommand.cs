using MediatR;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using StockApp.App.Dashboard.Query;
using StockApp.Entities;
using StockApp.Hub;

namespace StockApp.App.StockMovement.Command;

public record CreateStockMovementCommand : IRequest<CreateStockMovementCommandResponse>
{
    public int ProductId { get; init; }
    public StockMovementType Type { get; init; }
    public int Quantity { get; init; }
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
        var stockMovement = new Entities.StockMovement
        {
            ProductId = request.ProductId,
            CategoryId = product.CategoryId,
            Type = request.Type,
            Quantity = request.Quantity,
            Description = request.Description,
            CreatedAt = DateTime.UtcNow
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

        product.UpdatedAt = DateTime.UtcNow;

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

        return new CreateStockMovementCommandResponse(stockMovement.Id);
    }
}


