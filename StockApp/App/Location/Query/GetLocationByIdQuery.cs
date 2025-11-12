using MediatR;
using Microsoft.EntityFrameworkCore;

namespace StockApp.App.Location.Query;

public record GetLocationByIdQuery : IRequest<LocationDetailDto?>
{
    public int Id { get; init; }
}

public record LocationDetailDto
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public List<LocationProductDto> Products { get; init; } = new();
}

public record LocationProductDto
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string StockCode { get; init; } = string.Empty;
    public int StockQuantity { get; init; }
}

internal class GetLocationByIdQueryHandler : IRequestHandler<GetLocationByIdQuery, LocationDetailDto?>
{
    private readonly ApplicationDbContext _context;

    public GetLocationByIdQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<LocationDetailDto?> Handle(GetLocationByIdQuery request, CancellationToken cancellationToken)
    {
        var location = await _context.Locations
            .Include(l => l.Products)
            .FirstOrDefaultAsync(l => l.Id == request.Id, cancellationToken);

        if (location == null)
        {
            return null;
        }

        return new LocationDetailDto
        {
            Id = location.Id,
            Name = location.Name,
            Description = location.Description,
            Products = location.Products.Select(p => new LocationProductDto
            {
                Id = p.Id,
                Name = p.Name,
                StockCode = p.StockCode,
                StockQuantity = p.StockQuantity
            }).ToList()
        };
    }
}











