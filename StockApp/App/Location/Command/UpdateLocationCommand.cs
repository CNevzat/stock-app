using MediatR;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using StockApp.App.Dashboard.Query;
using StockApp.App.Location.Query;
using StockApp.Hub;
using StockApp.Services;
using StockApp.Common.Constants;

namespace StockApp.App.Location.Command;

public record UpdateLocationCommand : IRequest<UpdateLocationCommandResponse>
{
    public int LocationId { get; init; }
    public string? Name { get; init; }
    public string? Description { get; init; }
}

public record UpdateLocationCommandResponse
{
    public int LocationId { get; init; }
}

internal class UpdateLocationCommandHandler : IRequestHandler<UpdateLocationCommand, UpdateLocationCommandResponse>
{
    private readonly ApplicationDbContext _context;
    private readonly IMediator _mediator;
    private readonly IHubContext<StockHub> _hubContext;
    private readonly ICacheService _cacheService;

    public UpdateLocationCommandHandler(ApplicationDbContext context, IMediator mediator, IHubContext<StockHub> hubContext, ICacheService cacheService)
    {
        _context = context;
        _mediator = mediator;
        _hubContext = hubContext;
        _cacheService = cacheService;
    }

    public async Task<UpdateLocationCommandResponse> Handle(UpdateLocationCommand request, CancellationToken cancellationToken)
    {
        var location = await _context.Locations
            .FirstOrDefaultAsync(x => x.Id == request.LocationId, cancellationToken);

        if (location == null)
        {
            throw new KeyNotFoundException($"Location with Id {request.LocationId} not found.");
        }

        // Sadece gönderilen (null olmayan) alanları güncelle
        if (request.Name != null)
        {
            location.Name = request.Name;
        }
        
        if (request.Description != null)
        {
            location.Description = request.Description;
        }
        
        location.UpdatedAt = DateTime.UtcNow;
        _context.Locations.Update(location);
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

        // SignalR ile güncellenmiş lokasyonu tüm client'lara gönder
        try
        {
            var locationDetail = await _mediator.Send(new App.Location.Query.GetLocationByIdQuery { Id = location.Id }, cancellationToken);
            if (locationDetail != null)
            {
                await _hubContext.Clients.All.SendAsync("LocationUpdated", locationDetail, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"SignalR location updated gönderim hatası: {ex.Message}");
        }

        return new UpdateLocationCommandResponse
        {
            LocationId = location.Id
        };
    }
}












