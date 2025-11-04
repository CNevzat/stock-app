using MediatR;
using StockApp.App.Todo.Command;
using StockApp.App.Todo.Query;
using StockApp.Common.Models;
using StockApp.Entities;

namespace StockApp.ApiEndpoints;

public static class TodoEndpoints
{
    public static void MapTodos(this WebApplication app)
    {
        var group = app.MapGroup("/api/todos").WithTags("Todo");

        #region Get Todos

        group.MapGet("/", async (
            IMediator mediator,
            int pageNumber = 1,
            int pageSize = 10,
            TodoStatus? status = null,
            TodoPriority? priority = null) =>
        {
            var query = new GetTodosQuery
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                Status = status,
                Priority = priority
            };

            var result = await mediator.Send(query);
            return Results.Ok(result);
        })
        .Produces<PaginatedList<TodoDto>>(StatusCodes.Status200OK);

        #endregion

        #region Create Todo

        group.MapPost("/", async (
            IMediator mediator,
            CreateTodoCommand command) =>
        {
            var response = await mediator.Send(command);
            return Results.Ok(response);
        })
        .Produces<CreateTodoCommandResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest);

        #endregion

        #region Update Todo

        group.MapPut("/{id}", async (
            IMediator mediator,
            int id,
            UpdateTodoCommand command) =>
        {
            var updatedCommand = command with { Id = id };
            var response = await mediator.Send(updatedCommand);
            return Results.Ok(response);
        })
        .Produces<UpdateTodoCommandResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status400BadRequest);

        #endregion

        #region Delete Todo

        group.MapDelete("/{id}", async (
            IMediator mediator,
            int id) =>
        {
            var response = await mediator.Send(new DeleteTodoCommand(id));
            return Results.Ok(response);
        })
        .Produces<DeleteTodoCommandResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status400BadRequest);

        #endregion
    }
}

