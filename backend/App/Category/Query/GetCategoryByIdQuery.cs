using MediatR;
using Microsoft.EntityFrameworkCore;

namespace StockApp.App.Category.Query;

public record GetCategoryByIdQuery : IRequest<CategoryDetailDto?>
{
    public int Id { get; init; }
}

public record CategoryDetailDto
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public List<CategoryProductDto> Products { get; init; } = new();
}

public record CategoryProductDto
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public int StockQuantity { get; init; }
}

internal class GetCategoryByIdQueryHandler : IRequestHandler<GetCategoryByIdQuery, CategoryDetailDto?>
{
    private readonly ApplicationDbContext _context;

    public GetCategoryByIdQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<CategoryDetailDto?> Handle(GetCategoryByIdQuery request, CancellationToken cancellationToken)
    {
        var category = await _context.Categories
            .Include(c => c.Products)
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

        if (category == null)
        {
            return null;
        }

        return new CategoryDetailDto
        {
            Id = category.Id,
            Name = category.Name,
            Products = category.Products.Select(p => new CategoryProductDto
            {
                Id = p.Id,
                Name = p.Name,
                StockQuantity = p.StockQuantity
            }).ToList()
        };
    }
}

