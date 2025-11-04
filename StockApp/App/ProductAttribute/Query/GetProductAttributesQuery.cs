using MediatR;
using Microsoft.EntityFrameworkCore;
using StockApp.Common.Extensions;
using StockApp.Common.Models;
using System.Globalization;

namespace StockApp.App.ProductAttribute.Query;

public record GetProductAttributesQuery : IRequest<PaginatedList<ProductAttributeDto>>
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    public int? ProductId { get; init; }
    public string? SearchKey { get; init; }
}

public record ProductAttributeDto
{
    public int Id { get; init; }
    public int ProductId { get; init; }
    public string ProductName { get; init; } = string.Empty;
    public string Key { get; init; } = string.Empty;
    public string Value { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}

internal class GetProductAttributesQueryHandler : IRequestHandler<GetProductAttributesQuery, PaginatedList<ProductAttributeDto>>
{
    private readonly ApplicationDbContext _context;

    public GetProductAttributesQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PaginatedList<ProductAttributeDto>> Handle(GetProductAttributesQuery request, CancellationToken cancellationToken)
    {
        var query = _context.ProductAttributes
            .Include(pa => pa.Product)
            .AsQueryable();

        // Filter by product if provided
        if (request.ProductId.HasValue)
        {
            query = query.Where(pa => pa.ProductId == request.ProductId.Value);
        }

        // First, get all data for filtering (needed for Turkish culture-aware comparison)
        var allAttributes = await query.Select(pa => new ProductAttributeDto
        {
            Id = pa.Id,
            ProductId = pa.ProductId,
            ProductName = pa.Product.Name,
            Key = pa.Key,
            Value = pa.Value,
            CreatedAt = pa.CreatedAt,
            UpdatedAt = pa.UpdatedAt
        }).ToListAsync(cancellationToken);

        // Apply search filter with Turkish culture-aware comparison (C# side filtering)
        var filteredAttributes = allAttributes;
        if (!string.IsNullOrWhiteSpace(request.SearchKey))
        {
            var searchKey = request.SearchKey.Trim();
            
            // For Turkish characters, use CultureInfo with CompareOptions
            var turkishCulture = new CultureInfo("tr-TR");
            var compareOptions = CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace;
            
            filteredAttributes = allAttributes.Where(pa =>
                turkishCulture.CompareInfo.IndexOf(pa.Key, searchKey, compareOptions) >= 0 ||
                turkishCulture.CompareInfo.IndexOf(pa.Value, searchKey, compareOptions) >= 0 ||
                turkishCulture.CompareInfo.IndexOf(pa.ProductName, searchKey, compareOptions) >= 0
            ).ToList();
        }

        // En son güncellenen veya oluşturulan kayıt en üstte sırala (UpdatedAt varsa onu, yoksa CreatedAt'i kullan)
        var sortedAttributes = filteredAttributes
            .OrderByDescending(pa => pa.UpdatedAt ?? pa.CreatedAt)
            .ToList();

        // Apply pagination manually
        var totalCount = sortedAttributes.Count;
        var totalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize);
        var items = sortedAttributes
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList();

        return new PaginatedList<ProductAttributeDto>(
            items,
            totalCount,
            request.PageNumber,
            request.PageSize);
    }
}

