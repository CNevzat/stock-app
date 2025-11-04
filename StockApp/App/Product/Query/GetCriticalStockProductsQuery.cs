using MediatR;
using Microsoft.EntityFrameworkCore;
using StockApp;
using StockApp.Common.Extensions;
using StockApp.Common.Models;

namespace StockApp.App.Product.Query;

public record GetCriticalStockProductsQuery : IRequest<List<ProductDto>>;

internal class GetCriticalStockProductsQueryHandler : IRequestHandler<GetCriticalStockProductsQuery, List<ProductDto>>
{
    private readonly ApplicationDbContext _context;

    public GetCriticalStockProductsQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<ProductDto>> Handle(GetCriticalStockProductsQuery request, CancellationToken cancellationToken)
    {
        var products = await _context.Products
            .Include(p => p.Category)
            .Where(p => p.StockQuantity < p.LowStockThreshold)
            .OrderByDescending(p => p.LowStockThreshold - p.StockQuantity) // En kritik olanlar önce
            .ThenBy(p => p.Name)
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
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt
            })
            .ToListAsync(cancellationToken);

        return products;
    }
}

