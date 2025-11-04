using MediatR;
using StockApp.App.Category;
using StockApp.App.Category.Command;
using StockApp.App.Category.Query;
using StockApp.Common.Models;

namespace StockApp.ApiEndpoints;

public static class CategoryEndpoints
{
    public static void MapCategories(this WebApplication app)
    {
        var group = app.MapGroup("/api/categories").WithTags("Category");

        #region Get Categories

        group.MapGet("/", async (
            IMediator mediator,
            int pageNumber = 1,
            int pageSize = 10,
            string? searchTerm = null) =>
        {
            var query = new GetCategoriesQuery
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                SearchTerm = searchTerm
            };
            var categories = await mediator.Send(query);
            return Results.Ok(categories);
        })
        .Produces<PaginatedList<CategoryDto>>(StatusCodes.Status200OK);

        #endregion

        #region Get Category By Id

        group.MapGet("/by-id", async (
            IMediator mediator,
            int id) =>
        {
            var query = new GetCategoryByIdQuery { Id = id };
            var category = await mediator.Send(query);

            if (category == null)
            {
                throw new KeyNotFoundException($"Category with ID {id} not found.");
            }

            return Results.Ok(category);
        })
        .Produces<CategoryDetailDto>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);

        #endregion

        #region Create Category

        group.MapPost("/", async (
            IMediator mediator,
            CreateCategoryCommand command) =>
        {
            var response = await mediator.Send(command);
            return Results.Ok(response);
        })
        .Produces<CreateCategoryCommandResponse>(StatusCodes.Status200OK);

        #endregion

        #region Update Category

        group.MapPut("/", async (
            IMediator mediator,
            UpdateCategoryCommand command) =>
        {
            var response = await mediator.Send(command);
            return Results.Ok(response);
        })
        .Produces<UpdateCategoryCommandResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);

        #endregion

        #region Delete Category

        group.MapDelete("/", async (
            IMediator mediator,
            int id) =>
        {
            var command = new DeleteCategoryCommand { Id = id };
            var response = await mediator.Send(command);
            return Results.Ok(response);
        })
        .Produces<DeleteCategoryCommandResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);

        #endregion
    }
}

