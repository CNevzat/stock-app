using MediatR;
using Microsoft.EntityFrameworkCore;
using StockApp.Common.Extensions;
using StockApp.Common.Models;

namespace StockApp.App.Category.Query;

public record GetCategoriesQuery : IRequest<PaginatedList<CategoryDto>>
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    public string? SearchTerm { get; init; }
}

public record CategoryDto
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public int ProductCount { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}

internal class GetCategoriesQueryHandler : IRequestHandler<GetCategoriesQuery, PaginatedList<CategoryDto>>
{
    private readonly ApplicationDbContext _context;

    public GetCategoriesQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PaginatedList<CategoryDto>> Handle(GetCategoriesQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Categories
            .Include(c => c.Products)
            .AsQueryable();

        // Search by name if provided (case-insensitive using EF.Functions.Like)
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var searchTerm = $"%{request.SearchTerm.Trim()}%";
            query = query.Where(c => EF.Functions.Like(c.Name, searchTerm));
        }

        var categoryQuery = query.Select(c => new CategoryDto
        {
            Id = c.Id,
            Name = c.Name,
            ProductCount = c.Products
                .Select(x => x.StockQuantity).Sum(),
            CreatedAt = c.CreatedAt,
            UpdatedAt = c.UpdatedAt
        })
        // En son güncellenen veya oluşturulan kayıt en üstte (UpdatedAt varsa onu, yoksa CreatedAt'i kullan)
        .OrderByDescending(c => c.UpdatedAt ?? c.CreatedAt);

        return await categoryQuery.ToPaginatedListAsync(request.PageNumber, request.PageSize, cancellationToken);
    }
}

