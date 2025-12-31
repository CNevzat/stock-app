using MediatR;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using StockApp.App.Dashboard.Query;
using StockApp.Hub;
using StockApp.Services;
using StockApp.Common.Constants;

namespace StockApp.App.ProductAttribute.Command;

public record DeleteProductAttributeCommand : IRequest<DeleteProductAttributeCommandResponse>
{
    public int Id { get; init; }
}

public record DeleteProductAttributeCommandResponse
{
    public int ProductAttributeId { get; set; }
}

internal class DeleteProductAttributeCommandHandler : IRequestHandler<DeleteProductAttributeCommand, DeleteProductAttributeCommandResponse>
{
    private readonly ApplicationDbContext _context;
    private readonly IHubContext<StockHub> _hubContext;
    private readonly IMediator _mediator;
    private readonly ICacheService _cacheService;
    private readonly IElasticsearchService? _elasticsearchService;

    public DeleteProductAttributeCommandHandler(
        ApplicationDbContext context, 
        IHubContext<StockHub> hubContext, 
        IMediator mediator, 
        ICacheService cacheService,
        IElasticsearchService? elasticsearchService = null)
    {
        _context = context;
        _hubContext = hubContext;
        _mediator = mediator;
        _cacheService = cacheService;
        _elasticsearchService = elasticsearchService;
    }

    public async Task<DeleteProductAttributeCommandResponse> Handle(DeleteProductAttributeCommand request, CancellationToken cancellationToken)
    {
        var productAttribute = await _context.ProductAttributes
            .FirstOrDefaultAsync(pa => pa.Id == request.Id, cancellationToken);

        if (productAttribute == null)
        {
            throw new KeyNotFoundException($"ProductAttribute with ID {request.Id} not found.");
        }

        var deletedAttributeId = productAttribute.Id;

        _context.ProductAttributes.Remove(productAttribute);
        await _context.SaveChangesAsync(cancellationToken);

        // Cache'i invalidate et (dashboard stats değişti)
        await _cacheService.RemoveAsync(CacheKeys.DashboardStats, cancellationToken);

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

        // Elasticsearch'ten sil
        if (_elasticsearchService != null)
        {
            try
            {
                await _elasticsearchService.DeleteProductAttributeAsync(deletedAttributeId, cancellationToken);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Elasticsearch product attribute delete hatası: {ex.Message}");
            }
        }

        // SignalR ile silinen öznitelik ID'sini tüm client'lara gönder
        try
        {
            await _hubContext.Clients.All.SendAsync("ProductAttributeDeleted", deletedAttributeId, cancellationToken);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"SignalR product attribute deleted gönderim hatası: {ex.Message}");
        }

        return new DeleteProductAttributeCommandResponse
        {
            ProductAttributeId = deletedAttributeId
        };
    }
}

