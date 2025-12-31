using MediatR;
using Microsoft.EntityFrameworkCore;

namespace StockApp.App.Product.Query;

public record GetAllProductsQuery : IRequest<List<ProductDto>>;

internal class GetAllProductsQueryHandler : IRequestHandler<GetAllProductsQuery, List<ProductDto>>
{
    private readonly ApplicationDbContext _context;

    public GetAllProductsQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<ProductDto>> Handle(GetAllProductsQuery request, CancellationToken cancellationToken)
    {
        var products = await _context.Products
            .Include(p => p.Category)
            .Include(p => p.Location)
            .Select(p => new ProductDto
            {
                Id = p.Id,
                Name = p.Name,
                StockCode = p.StockCode,
                Description = p.Description,
                StockQuantity = p.StockQuantity,
                LowStockThreshold = p.LowStockThreshold,
                CategoryId = p.CategoryId,
                CategoryName = p.Category.Name,
                LocationId = p.LocationId,
                LocationName = p.Location != null ? p.Location.Name : null,
                ImagePath = p.ImagePath,
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt,
                CurrentPurchasePrice = p.CurrentPurchasePrice,
                CurrentSalePrice = p.CurrentSalePrice
            })
            .OrderBy(p => p.Name)
            .ToListAsync(cancellationToken);

        return products;
    }
}
