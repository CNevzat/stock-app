using MediatR;
using Microsoft.EntityFrameworkCore;

namespace StockApp.App.StockMovement.Query;

public record GetAllStockMovementsQuery : IRequest<List<StockMovementDto>>;

internal class GetAllStockMovementsQueryHandler : IRequestHandler<GetAllStockMovementsQuery, List<StockMovementDto>>
{
    private readonly ApplicationDbContext _context;

    public GetAllStockMovementsQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<StockMovementDto>> Handle(GetAllStockMovementsQuery request, CancellationToken cancellationToken)
    {
        return await _context.StockMovements
            .Include(sm => sm.Product)
            .Include(sm => sm.Category)
            .OrderByDescending(sm => sm.CreatedAt)
            .Select(sm => new StockMovementDto
            {
                Id = sm.Id,
                ProductId = sm.ProductId,
                ProductName = sm.Product.Name,
                CategoryId = sm.CategoryId,
                CategoryName = sm.Category.Name,
                Type = sm.Type,
                Quantity = sm.Quantity,
                UnitPrice = sm.UnitPrice,
                TotalValue = sm.UnitPrice * sm.Quantity,
                Description = sm.Description,
                CreatedAt = sm.CreatedAt,
                CurrentStockQuantity = sm.Product.StockQuantity,
                LowStockThreshold = sm.Product.LowStockThreshold
            })
            .ToListAsync(cancellationToken);
    }
}


