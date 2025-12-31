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

public record UpdateProductAttributeCommand : IRequest<UpdateProductAttributeCommandResponse>
{
    public int Id { get; init; }
    public string? Key { get; init; }
    public string? Value { get; init; }
}

public record UpdateProductAttributeCommandResponse
{
    public int ProductAttributeId { get; set; }
}

internal class UpdateProductAttributeCommandHandler : IRequestHandler<UpdateProductAttributeCommand, UpdateProductAttributeCommandResponse>
{
    private readonly ApplicationDbContext _context;
    private readonly IHubContext<StockHub> _hubContext;
    private readonly IMediator _mediator;
    private readonly ICacheService _cacheService;
    private readonly IElasticsearchService? _elasticsearchService;

    public UpdateProductAttributeCommandHandler(
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

    public async Task<UpdateProductAttributeCommandResponse> Handle(UpdateProductAttributeCommand request, CancellationToken cancellationToken)
    {
        var productAttribute = await _context.ProductAttributes
            .FirstOrDefaultAsync(pa => pa.Id == request.Id, cancellationToken);

        if (productAttribute == null)
        {
            throw new KeyNotFoundException($"ProductAttribute with ID {request.Id} not found.");
        }

        // Sadece gönderilen (null olmayan) alanları güncelle
        if (request.Key != null)
        {
            productAttribute.Key = request.Key;
        }

        if (request.Value != null)
        {
            productAttribute.Value = request.Value;
        }

        _context.ProductAttributes.Update(productAttribute);
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

        // SignalR ile güncellenmiş özniteliği tüm client'lara gönder
        try
        {
            var attributeDetail = await _mediator.Send(new GetProductAttributeByIdQuery { Id = productAttribute.Id }, cancellationToken);
            if (attributeDetail != null)
            {
                // Elasticsearch'i güncelle
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
                        await _elasticsearchService.UpdateProductAttributeAsync(productAttributeDto, cancellationToken);
                    }
                }

                await _hubContext.Clients.All.SendAsync("ProductAttributeUpdated", attributeDetail, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"SignalR product attribute updated gönderim hatası: {ex.Message}");
        }

        return new UpdateProductAttributeCommandResponse
        {
            ProductAttributeId = productAttribute.Id
        };
    }
}

