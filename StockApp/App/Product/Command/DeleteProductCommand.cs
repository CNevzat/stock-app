using MediatR;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using StockApp.App.Dashboard.Query;
using StockApp.Hub;
using StockApp.Services;

namespace StockApp.App.Product.Command;

public record DeleteProductCommand : IRequest<DeleteProductCommandResponse>
{
    public int Id { get; init; }
}

public record DeleteProductCommandResponse 
{
    public int ProductId { get; set; }
}
        

internal class DeleteProductCommandHandler : IRequestHandler<DeleteProductCommand, DeleteProductCommandResponse>
{
    private readonly ApplicationDbContext _context;
    private readonly IHubContext<StockHub> _hubContext;
    private readonly IMediator _mediator;
    private readonly IImageService _imageService;

    public DeleteProductCommandHandler(ApplicationDbContext context, IHubContext<StockHub> hubContext, IMediator mediator, IImageService imageService)
    {
        _context = context;
        _hubContext = hubContext;
        _mediator = mediator;
        _imageService = imageService;
    }

    public async Task<DeleteProductCommandResponse> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
    {
        var product = await _context.Products
            .Include(p => p.Attributes)
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

        if (product == null)
        {
            throw new KeyNotFoundException($"Product with ID {request.Id} not found.");
        }

        // İlişkili stok hareketlerini sil
        var stockMovements = await _context.StockMovements
            .Where(sm => sm.ProductId == request.Id)
            .ToListAsync(cancellationToken);
        
        if (stockMovements.Any())
        {
            _context.StockMovements.RemoveRange(stockMovements);
        }

        // Ürün özniteliklerini sil (navigation property üzerinden zaten yüklendi)
        if (product.Attributes.Any())
        {
            _context.ProductAttributes.RemoveRange(product.Attributes);
        }

        // Ürün resmini sil
        if (!string.IsNullOrEmpty(product.ImagePath))
        {
            _imageService.DeleteImage(product.ImagePath);
        }

        // Ürünü sil
        _context.Products.Remove(product);
        await _context.SaveChangesAsync(cancellationToken);

        var deletedProductId = product.Id;

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

        // SignalR ile silinen ürün ID'sini tüm client'lara gönder
        try
        {
            await _hubContext.Clients.All.SendAsync("ProductDeleted", deletedProductId, cancellationToken);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"SignalR product deleted gönderim hatası: {ex.Message}");
        }
        
        return new DeleteProductCommandResponse()
        {
            ProductId = deletedProductId
        };
    }
}

