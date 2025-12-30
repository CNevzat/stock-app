using MediatR;
using StockApp.App.Location;
using StockApp.App.Location.Command;
using StockApp.App.Location.Query;
using StockApp.Common.Models;

namespace StockApp.ApiEndpoints;

public static class LocationEndpoints
{
    public static void MapLocations(this WebApplication app)
    {
        var group = app.MapGroup("/api/locations").WithTags("Location");

        #region Get Locations

        group.MapGet("/", async (
            IMediator mediator,
            int pageNumber = 1,
            int pageSize = 10,
            string? searchTerm = null) =>
        {
            var query = new GetLocationsQuery
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                SearchTerm = searchTerm
            };
            var locations = await mediator.Send(query);
            return Results.Ok(locations);
        })
        .RequireAuthorization("CanViewLocations")
        .Produces<PaginatedList<LocationDto>>(StatusCodes.Status200OK);

        #endregion

        #region Get Location By Id

        group.MapGet("/by-id", async (
            IMediator mediator,
            int id) =>
        {
            var query = new GetLocationByIdQuery { Id = id };
            var location = await mediator.Send(query);

            if (location == null)
            {
                throw new KeyNotFoundException($"Location with ID {id} not found.");
            }

            return Results.Ok(location);
        })
        .RequireAuthorization("CanViewLocations")
        .Produces<LocationDetailDto>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);

        #endregion

        #region Create Location

        group.MapPost("/", async (
            IMediator mediator,
            CreateLocationCommand command) =>
        {
            var response = await mediator.Send(command);
            return Results.Ok(response);
        })
        .RequireAuthorization("CanManageLocations")
        .Produces<CreateLocationCommandResponse>(StatusCodes.Status200OK);

        #endregion

        #region Update Location

        group.MapPut("/", async (
            IMediator mediator,
            UpdateLocationCommand command) =>
        {
            var response = await mediator.Send(command);
            return Results.Ok(response);
        })
        .RequireAuthorization("CanManageLocations")
        .Produces<UpdateLocationCommandResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);

        #endregion

        #region Delete Location

        group.MapDelete("/", async (
            IMediator mediator,
            int id) =>
        {
            var command = new DeleteLocationCommand { Id = id };
            var response = await mediator.Send(command);
            return Results.Ok(response);
        })
        .RequireAuthorization("CanManageLocations")
        .Produces<DeleteLocationCommandResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);

        #endregion
    }
}












