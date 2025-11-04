using MediatR;
using Microsoft.EntityFrameworkCore;
using StockApp.Common.Extensions;
using StockApp.Common.Models;
using StockApp.Entities;

namespace StockApp.App.Todo.Query;

public record GetTodosQuery : IRequest<PaginatedList<TodoDto>>
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    public TodoStatus? Status { get; init; }
    public TodoPriority? Priority { get; init; }
}

public record TodoDto
{
    public int Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }
    public TodoStatus Status { get; init; }
    public string StatusText => Status switch
    {
        TodoStatus.Todo => "Yapılacak",
        TodoStatus.InProgress => "Devam Ediyor",
        TodoStatus.Completed => "Tamamlandı",
        _ => "Bilinmiyor"
    };
    public TodoPriority Priority { get; init; }
    public string PriorityText => Priority switch
    {
        TodoPriority.Low => "Düşük",
        TodoPriority.Medium => "Orta",
        TodoPriority.High => "Yüksek",
        _ => "Bilinmiyor"
    };
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}

internal class GetTodosQueryHandler : IRequestHandler<GetTodosQuery, PaginatedList<TodoDto>>
{
    private readonly ApplicationDbContext _context;

    public GetTodosQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PaginatedList<TodoDto>> Handle(GetTodosQuery request, CancellationToken cancellationToken)
    {
        var query = _context.TodoItems.AsQueryable();

        // Filter by status if provided
        if (request.Status.HasValue)
        {
            query = query.Where(t => t.Status == request.Status.Value);
        }

        // Filter by priority if provided
        if (request.Priority.HasValue)
        {
            query = query.Where(t => t.Priority == request.Priority.Value);
        }

        // Order by: önce tamamlanmayanlar, önceliğe göre, ardından tarihe göre
        query = query
            .OrderBy(t => t.Status == TodoStatus.Completed ? 1 : 0)
            .ThenByDescending(t => t.Priority)
            .ThenByDescending(t => t.CreatedAt);

        var todoQuery = query.Select(t => new TodoDto
        {
            Id = t.Id,
            Title = t.Title,
            Description = t.Description,
            Status = t.Status,
            Priority = t.Priority,
            CreatedAt = t.CreatedAt,
            UpdatedAt = t.UpdatedAt
        });

        return await todoQuery.ToPaginatedListAsync(request.PageNumber, request.PageSize, cancellationToken);
    }
}

