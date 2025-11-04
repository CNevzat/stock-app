using MediatR;
using Microsoft.EntityFrameworkCore;

namespace StockApp.App.ProductAttribute.Query;

public record GetProductAttributeByIdQuery : IRequest<ProductAttributeDetailDto?>
{
    public int Id { get; init; }
}

public record ProductAttributeDetailDto
{
    public int Id { get; init; }
    public int ProductId { get; init; }
    public string ProductName { get; init; } = string.Empty;
    public string Key { get; init; } = string.Empty;
    public string Value { get; init; } = string.Empty;
}

internal class GetProductAttributeByIdQueryHandler : IRequestHandler<GetProductAttributeByIdQuery, ProductAttributeDetailDto?>
{
    private readonly ApplicationDbContext _context;

    public GetProductAttributeByIdQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ProductAttributeDetailDto?> Handle(GetProductAttributeByIdQuery request, CancellationToken cancellationToken)
    {
        var productAttribute = await _context.ProductAttributes
            .Include(pa => pa.Product)
            .FirstOrDefaultAsync(pa => pa.Id == request.Id, cancellationToken);

        if (productAttribute == null)
        {
            return null;
        }

        return new ProductAttributeDetailDto
        {
            Id = productAttribute.Id,
            ProductId = productAttribute.ProductId,
            ProductName = productAttribute.Product.Name,
            Key = productAttribute.Key,
            Value = productAttribute.Value
        };
    }
}

