using MediatR;
using Microsoft.EntityFrameworkCore;
using StockApp.Common.Extensions;
using StockApp.Common.Models;

namespace StockApp.App.Location.Query;

public record GetLocationsQuery : IRequest<PaginatedList<LocationDto>>
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    public string? SearchTerm { get; init; }
}

public record LocationDto
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public int ProductCount { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}

internal class GetLocationsQueryHandler : IRequestHandler<GetLocationsQuery, PaginatedList<LocationDto>>
{
    private readonly ApplicationDbContext _context;

    public GetLocationsQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PaginatedList<LocationDto>> Handle(GetLocationsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Locations
            .Include(l => l.Products)
            .AsQueryable();

        // Search by name or description if provided (case-insensitive using EF.Functions.Like)
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var searchTerm = $"%{request.SearchTerm.Trim()}%";
            query = query.Where(l => EF.Functions.Like(l.Name, searchTerm) || 
                                     (l.Description != null && EF.Functions.Like(l.Description, searchTerm)));
        }

        var locationQuery = query.Select(l => new LocationDto
        {
            Id = l.Id,
            Name = l.Name,
            Description = l.Description,
            ProductCount = l.Products.Count,
            CreatedAt = l.CreatedAt,
            UpdatedAt = l.UpdatedAt
        })
        // En son güncellenen veya oluşturulan kayıt en üstte
        .OrderByDescending(l => l.UpdatedAt ?? l.CreatedAt);

        return await locationQuery.ToPaginatedListAsync(request.PageNumber, request.PageSize, cancellationToken);
    }
}










