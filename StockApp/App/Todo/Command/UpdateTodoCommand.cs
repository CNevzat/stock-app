using MediatR;
using Microsoft.EntityFrameworkCore;
using StockApp.Entities;

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

    public UpdateTodoCommandHandler(ApplicationDbContext context)
    {
        _context = context;
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

        return new UpdateTodoCommandResponse(todoItem.Id);
    }
}

