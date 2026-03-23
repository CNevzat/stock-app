using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StockApp.App.Category;
using StockApp.App.Category.Command;
using StockApp.App.Category.Query;
using StockApp.Common.Models;

namespace StockApp.Controllers;

[ApiController]
[Route("api/categories")]
[Tags("Category")]
public class CategoryController : ControllerBase
{
    private readonly IMediator _mediator;

    public CategoryController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [Authorize(Policy = "CanViewCategories")]
    public async Task<IActionResult> GetCategories(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? searchTerm = null)
    {
        var query = new GetCategoriesQuery
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            SearchTerm = searchTerm
        };
        var categories = await _mediator.Send(query);
        return Ok(categories);
    }

    [HttpGet("by-id")]
    [Authorize(Policy = "CanViewCategories")]
    public async Task<IActionResult> GetById([FromQuery] int id)
    {
        var query = new GetCategoryByIdQuery { Id = id };
        var category = await _mediator.Send(query);

        if (category == null)
        {
            throw new KeyNotFoundException($"Category with ID {id} not found.");
        }

        return Ok(category);
    }

    [HttpPost]
    [Authorize(Policy = "CanManageCategories")]
    public async Task<IActionResult> Create([FromBody] CreateCategoryCommand command)
    {
        var response = await _mediator.Send(command);
        return Ok(response);
    }

    [HttpPut]
    [Authorize(Policy = "CanManageCategories")]
    public async Task<IActionResult> Update([FromBody] UpdateCategoryCommand command)
    {
        var response = await _mediator.Send(command);
        return Ok(response);
    }

    [HttpDelete]
    [Authorize(Policy = "CanManageCategories")]
    public async Task<IActionResult> Delete([FromQuery] int id)
    {
        var command = new DeleteCategoryCommand { Id = id };
        var response = await _mediator.Send(command);
        return Ok(response);
    }
}
