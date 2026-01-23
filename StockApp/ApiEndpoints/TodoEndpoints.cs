using MediatR;
using System.Security.Claims;
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
            ClaimsPrincipal user,
            int pageNumber = 1,
            int pageSize = 10,
            TodoStatus? status = null,
            TodoPriority? priority = null) =>
        {
            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) return Results.Unauthorized();

            var query = new GetTodosQuery
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                Status = status,
                Priority = priority,
                UserId = userId
            };

            var result = await mediator.Send(query);
            return Results.Ok(result);
        })
        .RequireAuthorization("CanViewTodos")
        .Produces<PaginatedList<TodoDto>>(StatusCodes.Status200OK);

        group.MapGet("/calendar", async (
            IMediator mediator,
            ClaimsPrincipal user,
            DateTime? start = null,
            DateTime? end = null) =>
        {
            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) return Results.Unauthorized();

            var query = new GetCalendarTodosQuery
            {
                UserId = userId,
                Start = start,
                End = end
            };

            var result = await mediator.Send(query);
            return Results.Ok(result);
        })
        .RequireAuthorization("CanViewTodos")
        .Produces<List<TodoDto>>(StatusCodes.Status200OK);

        #endregion

        #region Create Todo

        group.MapPost("/", async (
            IMediator mediator,
            ClaimsPrincipal user,
            CreateTodoCommand command) =>
        {
            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) return Results.Unauthorized();

            var updatedCommand = command with { UserId = userId };
            var response = await mediator.Send(updatedCommand);
            return Results.Ok(response);
        })
        .RequireAuthorization("CanManageTodos")
        .Produces<CreateTodoCommandResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest);

        #endregion

        #region Update Todo

        group.MapPut("/{id}", async (
            IMediator mediator,
            ClaimsPrincipal user,
            int id,
            UpdateTodoCommand command) =>
        {
            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) return Results.Unauthorized();

            var updatedCommand = command with { Id = id, UserId = userId };
            var response = await mediator.Send(updatedCommand);
            return Results.Ok(response);
        })
        .RequireAuthorization("CanManageTodos")
        .Produces<UpdateTodoCommandResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status400BadRequest);

        #endregion

        #region Delete Todo

        group.MapDelete("/{id}", async (
            IMediator mediator,
            ClaimsPrincipal user,
            int id) =>
        {
            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) return Results.Unauthorized();

            var response = await mediator.Send(new DeleteTodoCommand(id, userId));
            return Results.Ok(response);
        })
        .RequireAuthorization("CanManageTodos")
        .Produces<DeleteTodoCommandResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status400BadRequest);

        #endregion
    }
}

