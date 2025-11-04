using MediatR;
using StockApp.Entities;

namespace StockApp.App.Todo.Command;

public record CreateTodoCommand : IRequest<CreateTodoCommandResponse>
{
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }
    public TodoStatus Status { get; init; } = TodoStatus.Todo;
    public TodoPriority Priority { get; init; } = TodoPriority.Medium;
}

public record CreateTodoCommandResponse(int Id);

internal class CreateTodoCommandHandler : IRequestHandler<CreateTodoCommand, CreateTodoCommandResponse>
{
    private readonly ApplicationDbContext _context;

    public CreateTodoCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<CreateTodoCommandResponse> Handle(CreateTodoCommand request, CancellationToken cancellationToken)
    {
        var todoItem = new TodoItem
        {
            Title = request.Title,
            Description = request.Description,
            Status = request.Status,
            Priority = request.Priority,
            CreatedAt = DateTime.UtcNow
        };

        _context.TodoItems.Add(todoItem);
        await _context.SaveChangesAsync(cancellationToken);

        return new CreateTodoCommandResponse(todoItem.Id);
    }
}

