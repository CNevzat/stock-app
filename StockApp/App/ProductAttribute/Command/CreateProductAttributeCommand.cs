using MediatR;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using StockApp.App.Dashboard.Query;
using StockApp.App.ProductAttribute.Query;
using StockApp.Hub;
using StockApp.Services;
using StockApp.Common.Constants;
using ProductAttributeDto = StockApp.App.ProductAttribute.Query.ProductAttributeDto;

namespace StockApp.App.ProductAttribute.Command;

public record CreateProductAttributeCommand : IRequest<CreateProductAttributeCommandResponse>
{
    public int ProductId { get; init; }
    public string Key { get; init; } = string.Empty;
    public string Value { get; init; } = string.Empty;
}

public record CreateProductAttributeCommandResponse
{
    public int ProductAttributeId { get; set; }
}

internal class CreateProductAttributeCommandHandler : IRequestHandler<CreateProductAttributeCommand, CreateProductAttributeCommandResponse>
{
    private readonly ApplicationDbContext _context;
    private readonly IHubContext<StockHub> _hubContext;
    private readonly IMediator _mediator;
    private readonly ICacheService _cacheService;
    private readonly IElasticsearchService? _elasticsearchService;

    public CreateProductAttributeCommandHandler(
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

    public async Task<CreateProductAttributeCommandResponse> Handle(CreateProductAttributeCommand request, CancellationToken cancellationToken)
    {
        var productAttribute = new Entities.ProductAttribute
        {
            ProductId = request.ProductId,
            Key = request.Key,
            Value = request.Value,
            CreatedAt = DateTime.UtcNow
        };

        _context.ProductAttributes.Add(productAttribute);
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

        // SignalR ile yeni özniteliği tüm client'lara gönder
        try
        {
            var attributeDetail = await _mediator.Send(new GetProductAttributeByIdQuery { Id = productAttribute.Id }, cancellationToken);
            if (attributeDetail != null)
            {
                // Elasticsearch'e index et
                if (_elasticsearchService != null)
                {
                    // Entity'den ProductAttributeDto oluştur
                    var productAttributeEntity = await _context.ProductAttributes
                        .Include(pa => pa.Product)
                        .FirstOrDefaultAsync(pa => pa.Id == productAttribute.Id, cancellationToken);
                    
                    if (productAttributeEntity != null)
                    {
                        var productAttributeDto = new ProductAttributeDto
                        {
                            Id = productAttributeEntity.Id,
                            ProductId = productAttributeEntity.ProductId,
                            ProductName = productAttributeEntity.Product.Name,
                            Key = productAttributeEntity.Key,
                            Value = productAttributeEntity.Value,
                            CreatedAt = productAttributeEntity.CreatedAt,
                            UpdatedAt = productAttributeEntity.UpdatedAt
                        };
                        await _elasticsearchService.IndexProductAttributeAsync(productAttributeDto, cancellationToken);
                    }
                }

                await _hubContext.Clients.All.SendAsync("ProductAttributeCreated", attributeDetail, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"SignalR product attribute created gönderim hatası: {ex.Message}");
        }

        return new CreateProductAttributeCommandResponse
        {
            ProductAttributeId = productAttribute.Id
        };
    }
}

