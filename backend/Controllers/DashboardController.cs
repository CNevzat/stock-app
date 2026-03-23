using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using StockApp.App.Dashboard.Query;
using StockApp.Hub;

namespace StockApp.Controllers;

[ApiController]
[Route("api/dashboard")]
[Tags("Dashboard")]
public class DashboardController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IHubContext<StockHub> _hubContext;

    public DashboardController(IMediator mediator, IHubContext<StockHub> hubContext)
    {
        _mediator = mediator;
        _hubContext = hubContext;
    }

    [HttpGet("stats")]
    [Authorize(Policy = "CanViewDashboard")]
    public async Task<IActionResult> GetStats()
    {
        var result = await _mediator.Send(new GetDashboardStatsQuery());

        await _hubContext.Clients.All.SendAsync("DashboardStatsUpdated", result);
        return Ok(result);
    }
}
