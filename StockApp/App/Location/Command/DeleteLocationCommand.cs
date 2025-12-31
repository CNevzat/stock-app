using MediatR;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using StockApp.App.Dashboard.Query;
using StockApp.Hub;
using StockApp.Services;
using StockApp.Common.Constants;

namespace StockApp.App.Location.Command;

public record DeleteLocationCommand : IRequest<DeleteLocationCommandResponse>
{
    public int Id { get; init; }
}

public record DeleteLocationCommandResponse
{
    public int LocationId { get; set; }
}

internal class DeleteLocationCommandHandler : IRequestHandler<DeleteLocationCommand, DeleteLocationCommandResponse>
{
    private readonly ApplicationDbContext _context;
    private readonly IHubContext<StockHub> _hubContext;
    private readonly IMediator _mediator;
    private readonly ICacheService _cacheService;

    public DeleteLocationCommandHandler(ApplicationDbContext context, IHubContext<StockHub> hubContext, IMediator mediator, ICacheService cacheService)
    {
        _context = context;
        _hubContext = hubContext;
        _mediator = mediator;
        _cacheService = cacheService;
    }

    public async Task<DeleteLocationCommandResponse> Handle(DeleteLocationCommand request, CancellationToken cancellationToken)
    {
        var location = await _context.Locations
            .FirstOrDefaultAsync(l => l.Id == request.Id, cancellationToken);

        if (location == null)
        {
            throw new KeyNotFoundException($"Location with ID {request.Id} not found.");
        }

        var deletedLocationId = location.Id;
        _context.Locations.Remove(location);
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

        // SignalR ile silinen lokasyon ID'sini tüm client'lara gönder
        try
        {
            await _hubContext.Clients.All.SendAsync("LocationDeleted", deletedLocationId, cancellationToken);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"SignalR location deleted gönderim hatası: {ex.Message}");
        }

        return new DeleteLocationCommandResponse
        {
            LocationId = location.Id
        };
    }
}












