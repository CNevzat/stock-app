using MediatR;
using Microsoft.AspNetCore.SignalR;
using StockApp.App.Dashboard.Query;
using StockApp.App.Location.Query;
using StockApp.Hub;
using StockApp.Services;
using StockApp.Common.Constants;

namespace StockApp.App.Location.Command;

public record CreateLocationCommand : IRequest<CreateLocationCommandResponse>
{
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
}

public record CreateLocationCommandResponse 
{
    public int LocationId { get; set; }
}

internal class CreateLocationCommandHandler : IRequestHandler<CreateLocationCommand, CreateLocationCommandResponse>
{
    private readonly ApplicationDbContext _context;
    private readonly IHubContext<StockHub> _hubContext;
    private readonly IMediator _mediator;
    private readonly ICacheService _cacheService;

    public CreateLocationCommandHandler(ApplicationDbContext context, IHubContext<StockHub> hubContext, IMediator mediator, ICacheService cacheService)
    {
        _context = context;
        _hubContext = hubContext;
        _mediator = mediator;
        _cacheService = cacheService;
    }

    public async Task<CreateLocationCommandResponse> Handle(CreateLocationCommand request, CancellationToken cancellationToken)
    {
        var location = new Entities.Location
        {
            Name = request.Name,
            Description = request.Description,
            CreatedAt = DateTime.UtcNow
        };

        _context.Locations.Add(location);
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

        // SignalR ile yeni lokasyonu tüm client'lara gönder
        try
        {
            var locationDetail = await _mediator.Send(new App.Location.Query.GetLocationByIdQuery { Id = location.Id }, cancellationToken);
            if (locationDetail != null)
            {
                await _hubContext.Clients.All.SendAsync("LocationCreated", locationDetail, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"SignalR location created gönderim hatası: {ex.Message}");
        }

        return new CreateLocationCommandResponse
        {
            LocationId = location.Id
        };
    }
}












