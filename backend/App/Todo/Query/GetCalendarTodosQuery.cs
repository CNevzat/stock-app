using MediatR;
using Microsoft.EntityFrameworkCore;
using StockApp.Entities;

namespace StockApp.App.Todo.Query;

public record GetCalendarTodosQuery : IRequest<List<TodoDto>>
{
    public string UserId { get; set; } = string.Empty;
    public DateTime? Start { get; init; }
    public DateTime? End { get; init; }
}

internal class GetCalendarTodosQueryHandler : IRequestHandler<GetCalendarTodosQuery, List<TodoDto>>
{
    private readonly ApplicationDbContext _context;

    public GetCalendarTodosQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<TodoDto>> Handle(GetCalendarTodosQuery request, CancellationToken cancellationToken)
    {
        var query = _context.TodoItems
            .Where(t => t.UserId == request.UserId)
            .AsQueryable();

        if (request.Start.HasValue)
        {
            query = query.Where(t => t.CompletedAt >= request.Start.Value || t.CreatedAt >= request.Start.Value);
        }

        if (request.End.HasValue)
        {
            query = query.Where(t => t.CreatedAt <= request.End.Value || t.CompletedAt <= request.End.Value);
        }

        return await query
            .OrderBy(t => t.CreatedAt)
            .Select(t => new TodoDto
            {
                Id = t.Id,
                Title = t.Title,
                Description = t.Description,
                Status = t.Status,
                Priority = t.Priority,
                CreatedAt = t.CreatedAt,
                UpdatedAt = t.UpdatedAt,
                CompletedAt = t.CompletedAt
            })
            .ToListAsync(cancellationToken);
    }
}
