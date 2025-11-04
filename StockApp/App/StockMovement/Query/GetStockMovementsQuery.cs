using MediatR;
using Microsoft.EntityFrameworkCore;
using StockApp.Common.Extensions;
using StockApp.Common.Models;
using StockApp.Entities;

namespace StockApp.App.StockMovement.Query;

public record GetStockMovementsQuery : IRequest<PaginatedList<StockMovementDto>>
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    public int? ProductId { get; init; }
    public int? CategoryId { get; init; }
    public StockMovementType? Type { get; init; }
}

public record StockMovementDto
{
    public int Id { get; init; }
    public int ProductId { get; init; }
    public string ProductName { get; init; } = string.Empty;
    public int CategoryId { get; init; }
    public string CategoryName { get; init; } = string.Empty;
    public StockMovementType Type { get; init; }
    public string TypeText => Type == StockMovementType.In ? "Giriş" : "Çıkış";
    public int Quantity { get; init; }
    public string? Description { get; init; }
    public DateTime CreatedAt { get; init; }
    public int CurrentStockQuantity { get; init; }
    public int LowStockThreshold { get; init; }
}

internal class GetStockMovementsQueryHandler : IRequestHandler<GetStockMovementsQuery, PaginatedList<StockMovementDto>>
{
    private readonly ApplicationDbContext _context;

    public GetStockMovementsQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PaginatedList<StockMovementDto>> Handle(GetStockMovementsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.StockMovements
            .Include(sm => sm.Product)
            .Include(sm => sm.Category)
            .AsQueryable();

        // Filter by product if provided
        if (request.ProductId.HasValue)
        {
            query = query.Where(sm => sm.ProductId == request.ProductId.Value);
        }

        // Filter by category if provided
        if (request.CategoryId.HasValue)
        {
            query = query.Where(sm => sm.CategoryId == request.CategoryId.Value);
        }

        // Filter by type if provided
        if (request.Type.HasValue)
        {
            query = query.Where(sm => sm.Type == request.Type.Value);
        }

        // Order by created date descending
        query = query.OrderByDescending(sm => sm.CreatedAt);

        var stockMovementQuery = query.Select(sm => new StockMovementDto
        {
            Id = sm.Id,
            ProductId = sm.ProductId,
            ProductName = sm.Product.Name,
            CategoryId = sm.CategoryId,
            CategoryName = sm.Category.Name,
            Type = sm.Type,
            Quantity = sm.Quantity,
            Description = sm.Description,
            CreatedAt = sm.CreatedAt,
            CurrentStockQuantity = sm.Product.StockQuantity,
            LowStockThreshold = sm.Product.LowStockThreshold
        });

        return await stockMovementQuery.ToPaginatedListAsync(request.PageNumber, request.PageSize, cancellationToken);
    }
}


