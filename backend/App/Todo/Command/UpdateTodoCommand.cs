using MediatR;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using StockApp.Entities;
using StockApp.Hub;

namespace StockApp.App.Todo.Command;

public sealed record UpdateTodoCommand : IRequest<UpdateTodoCommandResponse>
{
    public int Id { get; init; }
    public string? Title { get; init; }
    public string? Description { get; init; }
    public TodoStatus? Status { get; init; }
    public TodoPriority? Priority { get; init; }
    public string UserId { get; set; } = string.Empty;
}

public sealed record UpdateTodoCommandResponse(int Id);

internal sealed class UpdateTodoCommandHandler : IRequestHandler<UpdateTodoCommand, UpdateTodoCommandResponse>
{
    private readonly ApplicationDbContext _context;
    private readonly IHubContext<StockHub> _hubContext;

    public UpdateTodoCommandHandler(ApplicationDbContext context, IHubContext<StockHub> hubContext)
    {
        _context = context;
        _hubContext = hubContext;
    }

    public async Task<UpdateTodoCommandResponse> Handle(UpdateTodoCommand request, CancellationToken cancellationToken)
    {
        var todoItem = await _context.TodoItems
            .FirstOrDefaultAsync(t => t.Id == request.Id && t.UserId == request.UserId, cancellationToken);

        if (todoItem == null)
        {
            throw new KeyNotFoundException($"Todo item bulunamadı veya bu işlem için yetkiniz yok: {request.Id}");
        }

        if (request.Title != null)
        {
            if (!string.IsNullOrWhiteSpace(request.Title))
            {
                todoItem.Title = request.Title;
            }
        }

        if (request.Status.HasValue)
        {
            // Eğer durum Tamamlandı'ya (3) çekiliyorsa ve daha önce tamamlanmamışsa CompletedAt set et
            if (request.Status.Value == TodoStatus.Completed && todoItem.Status != TodoStatus.Completed)
            {
                todoItem.CompletedAt = DateTime.UtcNow;
            }
            // Eğer Tamamlandı'dan geri çekiliyorsa CompletedAt temizle
            else if (request.Status.Value != TodoStatus.Completed && todoItem.Status == TodoStatus.Completed)
            {
                todoItem.CompletedAt = null;
            }
            
            todoItem.Status = request.Status.Value;
        }

        if (request.Priority.HasValue)
        {
            todoItem.Priority = request.Priority.Value;
        }

        todoItem.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        // SignalR ile güncellenmiş todo'yu sadece ilgili kullanıcıya gönder
        try
        {
            var todoDetail = new App.Todo.Query.TodoDto
            {
                Id = todoItem.Id,
                Title = todoItem.Title,
                Description = todoItem.Description,
                Status = todoItem.Status,
                Priority = todoItem.Priority,
                CreatedAt = todoItem.CreatedAt,
                UpdatedAt = todoItem.UpdatedAt,
                CompletedAt = todoItem.CompletedAt
            };
            await _hubContext.Clients.User(request.UserId).SendAsync("TodoUpdated", todoDetail, cancellationToken);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"SignalR todo updated gönderim hatası: {ex.Message}");
        }

        return new UpdateTodoCommandResponse(todoItem.Id);
    }
}

