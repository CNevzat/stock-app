using MediatR;
using Microsoft.AspNetCore.SignalR;
using StockApp.App.Dashboard.Query;
using StockApp.Hub;

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

    public CreateLocationCommandHandler(ApplicationDbContext context, IHubContext<StockHub> hubContext, IMediator mediator)
    {
        _context = context;
        _hubContext = hubContext;
        _mediator = mediator;
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

        return new CreateLocationCommandResponse
        {
            LocationId = location.Id
        };
    }
}



