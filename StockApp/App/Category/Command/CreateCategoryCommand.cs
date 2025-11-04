using MediatR;
using Microsoft.AspNetCore.SignalR;
using StockApp.App.Category.Query;
using StockApp.App.Dashboard.Query;
using StockApp.Hub;

namespace StockApp.App.Category.Command;

public record CreateCategoryCommand : IRequest<CreateCategoryCommandResponse>
{
    public string Name { get; init; } = string.Empty;
}

public record CreateCategoryCommandResponse 
{
    public int CategoryId { get; set; }
}

internal class CreateCategoryCommandHandler : IRequestHandler<CreateCategoryCommand, CreateCategoryCommandResponse>
{
    private readonly ApplicationDbContext _context;
    private readonly IHubContext<StockHub> _hubContext;
    private readonly IMediator _mediator;

    public CreateCategoryCommandHandler(ApplicationDbContext context, IHubContext<StockHub> hubContext, IMediator mediator)
    {
        _context = context;
        _hubContext = hubContext;
        _mediator = mediator;
    }

    public async Task<CreateCategoryCommandResponse> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = new Entities.Category
        {
            Name = request.Name,
            CreatedAt = DateTime.UtcNow
        };

        _context.Categories.Add(category);
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

        // SignalR ile yeni kategoriyi tüm client'lara gönder
        try
        {
            var categoryDetail = await _mediator.Send(new GetCategoryByIdQuery { Id = category.Id }, cancellationToken);
            if (categoryDetail != null)
            {
                await _hubContext.Clients.All.SendAsync("CategoryCreated", categoryDetail, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"SignalR category created gönderim hatası: {ex.Message}");
        }

        return new CreateCategoryCommandResponse
        {
            CategoryId = category.Id
        };
    }
}