using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StockApp.App.Location;
using StockApp.App.Location.Command;
using StockApp.App.Location.Query;
using StockApp.Common.Models;

namespace StockApp.Controllers;

[ApiController]
[Route("api/locations")]
[Tags("Location")]
public class LocationController : ControllerBase
{
    private readonly IMediator _mediator;

    public LocationController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [Authorize(Policy = "CanViewLocations")]
    public async Task<IActionResult> GetLocations(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? searchTerm = null)
    {
        var query = new GetLocationsQuery
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            SearchTerm = searchTerm
        };
        var locations = await _mediator.Send(query);
        return Ok(locations);
    }

    [HttpGet("by-id")]
    [Authorize(Policy = "CanViewLocations")]
    public async Task<IActionResult> GetById([FromQuery] int id)
    {
        var query = new GetLocationByIdQuery { Id = id };
        var location = await _mediator.Send(query);

        if (location == null)
        {
            throw new KeyNotFoundException($"Location with ID {id} not found.");
        }

        return Ok(location);
    }

    [HttpPost]
    [Authorize(Policy = "CanManageLocations")]
    public async Task<IActionResult> Create([FromBody] CreateLocationCommand command)
    {
        var response = await _mediator.Send(command);
        return Ok(response);
    }

    [HttpPut]
    [Authorize(Policy = "CanManageLocations")]
    public async Task<IActionResult> Update([FromBody] UpdateLocationCommand command)
    {
        var response = await _mediator.Send(command);
        return Ok(response);
    }

    [HttpDelete]
    [Authorize(Policy = "CanManageLocations")]
    public async Task<IActionResult> Delete([FromQuery] int id)
    {
        var command = new DeleteLocationCommand { Id = id };
        var response = await _mediator.Send(command);
        return Ok(response);
    }
}
