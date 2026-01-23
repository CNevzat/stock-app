using MediatR;
using Microsoft.AspNetCore.SignalR;
using StockApp.Entities;
using StockApp.Hub;

namespace StockApp.App.Todo.Command;

public record CreateTodoCommand : IRequest<CreateTodoCommandResponse>
{
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }
    public TodoStatus Status { get; init; } = TodoStatus.Todo;
    public TodoPriority Priority { get; init; } = TodoPriority.Medium;
    public string UserId { get; set; } = string.Empty;
}

public record CreateTodoCommandResponse(int Id);

internal class CreateTodoCommandHandler : IRequestHandler<CreateTodoCommand, CreateTodoCommandResponse>
{
    private readonly ApplicationDbContext _context;
    private readonly IHubContext<StockHub> _hubContext;

    public CreateTodoCommandHandler(ApplicationDbContext context, IHubContext<StockHub> hubContext)
    {
        _context = context;
        _hubContext = hubContext;
    }

    public async Task<CreateTodoCommandResponse> Handle(CreateTodoCommand request, CancellationToken cancellationToken)
    {
        var todoItem = new TodoItem
        {
            Title = request.Title,
            Description = request.Description,
            Status = request.Status,
            Priority = request.Priority,
            CreatedAt = DateTime.UtcNow,
            UserId = request.UserId
        };

        if (todoItem.Status == TodoStatus.Completed)
        {
            todoItem.CompletedAt = DateTime.UtcNow;
        }

        _context.TodoItems.Add(todoItem);
        await _context.SaveChangesAsync(cancellationToken);

        // SignalR ile yeni todo'yu sadece ilgili kullanıcıya gönder
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
            await _hubContext.Clients.User(request.UserId).SendAsync("TodoCreated", todoDetail, cancellationToken);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"SignalR todo created gönderim hatası: {ex.Message}");
        }

        return new CreateTodoCommandResponse(todoItem.Id);
    }
}

