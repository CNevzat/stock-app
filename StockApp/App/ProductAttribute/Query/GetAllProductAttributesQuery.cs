using MediatR;
using Microsoft.EntityFrameworkCore;

namespace StockApp.App.ProductAttribute.Query;

public record GetAllProductAttributesQuery : IRequest<List<ProductAttributeDto>>;

internal class GetAllProductAttributesQueryHandler : IRequestHandler<GetAllProductAttributesQuery, List<ProductAttributeDto>>
{
    private readonly ApplicationDbContext _context;

    public GetAllProductAttributesQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<ProductAttributeDto>> Handle(GetAllProductAttributesQuery request, CancellationToken cancellationToken)
    {
        var attributes = await _context.ProductAttributes
            .Include(pa => pa.Product)
            .Select(pa => new ProductAttributeDto
            {
                Id = pa.Id,
                ProductId = pa.ProductId,
                ProductName = pa.Product.Name,
                Key = pa.Key,
                Value = pa.Value
            })
            .OrderBy(pa => pa.ProductName)
            .ThenBy(pa => pa.Key)
            .ToListAsync(cancellationToken);

        return attributes;
    }
}
