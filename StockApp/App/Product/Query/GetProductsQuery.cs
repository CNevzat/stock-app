using MediatR;
using Microsoft.EntityFrameworkCore;
using StockApp.Common.Extensions;
using StockApp.Common.Models;

namespace StockApp.App.Product.Query;

public record GetProductsQuery : IRequest<PaginatedList<ProductDto>>
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    public int? CategoryId { get; init; }
    public int? LocationId { get; init; }
    public string? SearchTerm { get; init; }
}

public record ProductDto
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
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public decimal CurrentPurchasePrice { get; init; }
    public decimal CurrentSalePrice { get; init; }
}

internal class GetProductsQueryHandler : IRequestHandler<GetProductsQuery, PaginatedList<ProductDto>>
{
    private readonly ApplicationDbContext _context;

    public GetProductsQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PaginatedList<ProductDto>> Handle(GetProductsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Products
            .Include(p => p.Category)
            .Include(p => p.Location)
            .AsQueryable();

        // Filter by category if provided
        if (request.CategoryId.HasValue)
        {
            query = query.Where(p => p.CategoryId == request.CategoryId.Value);
        }

        // Filter by location if provided
        if (request.LocationId.HasValue)
        {
            query = query.Where(p => p.LocationId == request.LocationId.Value);
        }

        // Search by name, description, stock code, or location name if provided (case-insensitive)
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var searchTermLower = request.SearchTerm.ToLower();
            query = query.Where(p => 
                p.Name.ToLower().Contains(searchTermLower) || 
                p.Description.ToLower().Contains(searchTermLower) ||
                p.StockCode.ToLower().Contains(searchTermLower) ||
                (p.Location != null && p.Location.Name.ToLower().Contains(searchTermLower)));
        }

        var productQuery = query.Select(p => new ProductDto
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
        // En son güncellenen veya oluşturulan kayıt en üstte (UpdatedAt varsa onu, yoksa CreatedAt'i kullan)
        .OrderByDescending(p => p.UpdatedAt ?? p.CreatedAt);

        return await productQuery.ToPaginatedListAsync(request.PageNumber, request.PageSize, cancellationToken);
    }
}

