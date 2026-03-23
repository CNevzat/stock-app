using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StockApp.App.Todo.Command;
using StockApp.App.Todo.Query;
using StockApp.Common.Models;
using StockApp.Entities;

namespace StockApp.Controllers;

[ApiController]
[Route("api/todos")]
[Tags("Todo")]
public class TodoController : ControllerBase
{
    private readonly IMediator _mediator;

    public TodoController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [Authorize(Policy = "CanViewTodos")]
    public async Task<IActionResult> GetTodos(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] TodoStatus? status = null,
        [FromQuery] TodoPriority? priority = null)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var query = new GetTodosQuery
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            Status = status,
            Priority = priority,
            UserId = userId
        };

        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpGet("calendar")]
    [Authorize(Policy = "CanViewTodos")]
    public async Task<IActionResult> GetCalendarTodos(
        [FromQuery] DateTime? start = null,
        [FromQuery] DateTime? end = null)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var query = new GetCalendarTodosQuery
        {
            UserId = userId,
            Start = start,
            End = end
        };

        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Policy = "CanManageTodos")]
    public async Task<IActionResult> Create([FromBody] CreateTodoCommand command)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var updatedCommand = command with { UserId = userId };
        var response = await _mediator.Send(updatedCommand);
        return Ok(response);
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = "CanManageTodos")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateTodoCommand command)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var updatedCommand = command with { Id = id, UserId = userId };
        var response = await _mediator.Send(updatedCommand);
        return Ok(response);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Policy = "CanManageTodos")]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var response = await _mediator.Send(new DeleteTodoCommand(id, userId));
        return Ok(response);
    }
}
