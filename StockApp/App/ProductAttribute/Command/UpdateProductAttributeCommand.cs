using MediatR;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using StockApp.App.Dashboard.Query;
using StockApp.App.ProductAttribute.Query;
using StockApp.Hub;

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

    public UpdateProductAttributeCommandHandler(ApplicationDbContext context, IHubContext<StockHub> hubContext, IMediator mediator)
    {
        _context = context;
        _hubContext = hubContext;
        _mediator = mediator;
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

