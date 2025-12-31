using MediatR;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using StockApp.Entities;
using StockApp.Hub;

namespace StockApp.App.Todo.Command;

public record UpdateTodoCommand : IRequest<UpdateTodoCommandResponse>
{
    public int Id { get; init; }
    public string? Title { get; init; }
    public string? Description { get; init; }
    public TodoStatus? Status { get; init; }
    public TodoPriority? Priority { get; init; }
}

public record UpdateTodoCommandResponse(int Id);

internal class UpdateTodoCommandHandler : IRequestHandler<UpdateTodoCommand, UpdateTodoCommandResponse>
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
        var todoItem = await _context.TodoItems.FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken);

        if (todoItem == null)
        {
            throw new KeyNotFoundException($"Todo item bulunamadı: {request.Id}");
        }

        if (!string.IsNullOrWhiteSpace(request.Title))
        {
            todoItem.Title = request.Title;
        }

        if (request.Description != null)
        {
            todoItem.Description = request.Description;
        }

        if (request.Status.HasValue)
        {
            todoItem.Status = request.Status.Value;
        }

        if (request.Priority.HasValue)
        {
            todoItem.Priority = request.Priority.Value;
        }

        todoItem.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        // SignalR ile güncellenmiş todo'yu tüm client'lara gönder
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
                UpdatedAt = todoItem.UpdatedAt
            };
            await _hubContext.Clients.All.SendAsync("TodoUpdated", todoDetail, cancellationToken);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"SignalR todo updated gönderim hatası: {ex.Message}");
        }

        return new UpdateTodoCommandResponse(todoItem.Id);
    }
}

