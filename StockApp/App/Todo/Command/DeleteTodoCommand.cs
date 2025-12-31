using MediatR;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using StockApp.Hub;

namespace StockApp.App.Todo.Command;

public record DeleteTodoCommand(int Id) : IRequest<DeleteTodoCommandResponse>;

public record DeleteTodoCommandResponse(int Id);

internal class DeleteTodoCommandHandler : IRequestHandler<DeleteTodoCommand, DeleteTodoCommandResponse>
{
    private readonly ApplicationDbContext _context;
    private readonly IHubContext<StockHub> _hubContext;

    public DeleteTodoCommandHandler(ApplicationDbContext context, IHubContext<StockHub> hubContext)
    {
        _context = context;
        _hubContext = hubContext;
    }

    public async Task<DeleteTodoCommandResponse> Handle(DeleteTodoCommand request, CancellationToken cancellationToken)
    {
        var todoItem = await _context.TodoItems.FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken);

        if (todoItem == null)
        {
            throw new KeyNotFoundException($"Todo item bulunamadı: {request.Id}");
        }

        var deletedTodoId = todoItem.Id;
        _context.TodoItems.Remove(todoItem);
        await _context.SaveChangesAsync(cancellationToken);

        // SignalR ile silinen todo ID'sini tüm client'lara gönder
        try
        {
            await _hubContext.Clients.All.SendAsync("TodoDeleted", deletedTodoId, cancellationToken);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"SignalR todo deleted gönderim hatası: {ex.Message}");
        }

        return new DeleteTodoCommandResponse(deletedTodoId);
    }
}


