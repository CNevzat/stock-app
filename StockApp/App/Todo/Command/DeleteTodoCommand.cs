using MediatR;
using Microsoft.EntityFrameworkCore;

namespace StockApp.App.Todo.Command;

public record DeleteTodoCommand(int Id) : IRequest<DeleteTodoCommandResponse>;

public record DeleteTodoCommandResponse(int Id);

internal class DeleteTodoCommandHandler : IRequestHandler<DeleteTodoCommand, DeleteTodoCommandResponse>
{
    private readonly ApplicationDbContext _context;

    public DeleteTodoCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<DeleteTodoCommandResponse> Handle(DeleteTodoCommand request, CancellationToken cancellationToken)
    {
        var todoItem = await _context.TodoItems.FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken);

        if (todoItem == null)
        {
            throw new KeyNotFoundException($"Todo item bulunamadı: {request.Id}");
        }

        _context.TodoItems.Remove(todoItem);
        await _context.SaveChangesAsync(cancellationToken);

        return new DeleteTodoCommandResponse(todoItem.Id);
    }
}


