using MediatR;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using StockApp.Hub;

namespace StockApp.App.Todo.Command;

public record DeleteTodoCommand(int Id, string UserId = "") : IRequest<DeleteTodoCommandResponse>;

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
        var todoItem = await _context.TodoItems
            .FirstOrDefaultAsync(t => t.Id == request.Id && t.UserId == request.UserId, cancellationToken);

        if (todoItem == null)
        {
            throw new KeyNotFoundException($"Todo item bulunamadı veya bu işlem için yetkiniz yok: {request.Id}");
        }

        var deletedTodoId = todoItem.Id;
        _context.TodoItems.Remove(todoItem);
        await _context.SaveChangesAsync(cancellationToken);

        // SignalR ile silinen todo ID'sini sadece ilgili kullanıcıya gönder
        try
        {
            await _hubContext.Clients.User(request.UserId).SendAsync("TodoDeleted", deletedTodoId, cancellationToken);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"SignalR todo deleted gönderim hatası: {ex.Message}");
        }

        return new DeleteTodoCommandResponse(deletedTodoId);
    }
}


