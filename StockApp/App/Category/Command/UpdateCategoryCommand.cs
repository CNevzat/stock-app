using MediatR;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using StockApp.App.Category.Query;
using StockApp.App.Dashboard.Query;
using StockApp.Hub;

namespace StockApp.App.Category.Command;

public record UpdateCategoryCommand : IRequest<UpdateCategoryCommandResponse>
{
    public int CategoryId { get; init; }
    public string? Name { get; init; }
}

public record UpdateCategoryCommandResponse
{
    public int CategoryId { get; init; }
}

internal class UpdateCategoryCommandHandler : IRequestHandler<UpdateCategoryCommand, UpdateCategoryCommandResponse>
{
    private readonly ApplicationDbContext _context;
    private readonly IMediator _mediator;
    private readonly IHubContext<StockHub> _hubContext;

    public UpdateCategoryCommandHandler(ApplicationDbContext context, IMediator mediator, IHubContext<StockHub> hubContext)
    {
        _context = context;
        _mediator = mediator;
        _hubContext = hubContext;
    }

    public async Task<UpdateCategoryCommandResponse> Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await _context.Categories
            .FirstOrDefaultAsync(x => x.Id == request.CategoryId, cancellationToken);

        if (category == null)
        {
            throw new KeyNotFoundException($"Category with Id {request.CategoryId} not found.");
        }


        
        // Sadece gönderilen (null olmayan) alanları güncelle
        if (request.Name != null)
        {
            category.Name = request.Name;
        }
        
        category.UpdatedAt = DateTime.UtcNow;
        _context.Categories.Update(category);
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

        // SignalR ile güncellenmiş kategoriyi tüm client'lara gönder
        try
        {
            var categoryDetail = await _mediator.Send(new GetCategoryByIdQuery { Id = category.Id }, cancellationToken);
            if (categoryDetail != null)
            {
                await _hubContext.Clients.All.SendAsync("CategoryUpdated", categoryDetail, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"SignalR category updated gönderim hatası: {ex.Message}");
        }

        return new UpdateCategoryCommandResponse
        {
            CategoryId = category.Id
        };
    }
}