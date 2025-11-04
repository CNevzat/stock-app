using MediatR;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using StockApp.App.Dashboard.Query;
using StockApp.Hub;

namespace StockApp.App.Category.Command;

public record DeleteCategoryCommand : IRequest<DeleteCategoryCommandResponse>
{
    public int Id { get; init; }
}

public record DeleteCategoryCommandResponse
{
    public int CategoryId { get; set; }
}

internal class DeleteCategoryCommandHandler : IRequestHandler<DeleteCategoryCommand, DeleteCategoryCommandResponse>
{
    private readonly ApplicationDbContext _context;
    private readonly IHubContext<StockHub> _hubContext;
    private readonly IMediator _mediator;

    public DeleteCategoryCommandHandler(ApplicationDbContext context, IHubContext<StockHub> hubContext, IMediator mediator)
    {
        _context = context;
        _hubContext = hubContext;
        _mediator = mediator;
    }

    public async Task<DeleteCategoryCommandResponse> Handle(DeleteCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await _context.Categories
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

        if (category == null)
        {
            throw new KeyNotFoundException($"Category with ID {request.Id} not found.");
        }

        var deletedCategoryId = category.Id;

        _context.Categories.Remove(category);
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

        // SignalR ile silinen kategori ID'sini tüm client'lara gönder
        try
        {
            await _hubContext.Clients.All.SendAsync("CategoryDeleted", deletedCategoryId, cancellationToken);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"SignalR category deleted gönderim hatası: {ex.Message}");
        }

        return new DeleteCategoryCommandResponse
        {
            CategoryId = deletedCategoryId
        };
    }
}

