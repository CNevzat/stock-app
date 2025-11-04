using MediatR;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using StockApp.App.Dashboard.Query;
using StockApp.Hub;

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

    public DeleteProductAttributeCommandHandler(ApplicationDbContext context, IHubContext<StockHub> hubContext, IMediator mediator)
    {
        _context = context;
        _hubContext = hubContext;
        _mediator = mediator;
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

