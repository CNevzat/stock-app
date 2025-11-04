using MediatR;
using StockApp.App.StockMovement.Command;
using StockApp.App.StockMovement.Query;
using StockApp.Common.Models;
using StockApp.Entities;

namespace StockApp.ApiEndpoints;

public static class StockMovementEndpoints
{
    public static void MapStockMovements(this WebApplication app)
    {
        var group = app.MapGroup("/api/stock-movements").WithTags("StockMovement");

        #region Get Stock Movements

        group.MapGet("/", async (
            IMediator mediator,
            int pageNumber = 1,
            int pageSize = 10,
            int? productId = null,
            int? categoryId = null,
            StockMovementType? type = null) =>
        {
            var query = new GetStockMovementsQuery
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                ProductId = productId,
                CategoryId = categoryId,
                Type = type
            };
            var movements = await mediator.Send(query);
            return Results.Ok(movements);
        })
        .Produces<PaginatedList<StockMovementDto>>(StatusCodes.Status200OK);

        #endregion

        #region Create Stock Movement

        group.MapPost("/", async (
            IMediator mediator,
            CreateStockMovementCommand command) =>
        {
            var response = await mediator.Send(command);
            return Results.Ok(response);
        })
        .Produces<CreateStockMovementCommandResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status400BadRequest);

        #endregion
    }
}

