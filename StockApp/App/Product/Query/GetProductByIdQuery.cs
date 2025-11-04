using MediatR;
using Microsoft.EntityFrameworkCore;

namespace StockApp.App.Product.Query;

public record GetProductByIdQuery : IRequest<ProductDetailDto?>
{
    public int Id { get; init; }
}

public record ProductDetailDto
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string StockCode { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public int StockQuantity { get; init; }
    public int LowStockThreshold { get; init; }
    public int CategoryId { get; init; }
    public string CategoryName { get; init; } = string.Empty;
    public int? LocationId { get; init; }
    public string? LocationName { get; init; }
    public string? ImagePath { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
    public List<ProductAttributeDto> Attributes { get; init; } = new();
}

public record ProductAttributeDto
{
    public int Id { get; init; }
    public string Key { get; init; } = string.Empty;
    public string Value { get; init; } = string.Empty;
}

internal class GetProductByIdQueryHandler : IRequestHandler<GetProductByIdQuery, ProductDetailDto?>
{
    private readonly ApplicationDbContext _context;

    public GetProductByIdQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ProductDetailDto?> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
    {
        var product = await _context.Products
            .Include(p => p.Category)
            .Include(p => p.Location)
            .Include(p => p.Attributes)
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

        if (product == null)
        {
            return null;
        }

        return new ProductDetailDto
        {
            Id = product.Id,
            Name = product.Name,
            StockCode = product.StockCode,
            Description = product.Description,
            StockQuantity = product.StockQuantity,
            LowStockThreshold = product.LowStockThreshold,
            CategoryId = product.CategoryId,
            CategoryName = product.Category.Name,
            LocationId = product.LocationId,
            LocationName = product.Location != null ? product.Location.Name : null,
            ImagePath = product.ImagePath,
            CreatedAt = product.CreatedAt,
            UpdatedAt = product.UpdatedAt,
            Attributes = product.Attributes.Select(a => new ProductAttributeDto
            {
                Id = a.Id,
                Key = a.Key,
                Value = a.Value
            }).ToList()
        };
    }
}

