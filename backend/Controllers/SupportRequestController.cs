using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StockApp.App.SupportRequest.Command;
using StockApp.App.SupportRequest.Query;
using StockApp.Entities;

namespace StockApp.Controllers;

[ApiController]
[Route("api/support-requests")]
[Authorize(Roles = "Admin")]
[Tags("SupportRequests")]
public class SupportRequestController : ControllerBase
{
    private readonly IMediator _mediator;

    public SupportRequestController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        SupportRequestStatus? statusEnum = null;
        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<SupportRequestStatus>(status, true, out var parsed))
            statusEnum = parsed;

        var result = await _mediator.Send(new GetSupportRequestsQuery(statusEnum, page, pageSize), cancellationToken);
        return Ok(result);
    }

    [HttpPatch("{id:int}/status")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateStatusRequest request, CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<SupportRequestStatus>(request.Status, true, out var status))
            return BadRequest(new { message = "Geçersiz durum değeri. 'Pending' veya 'Replied' olmalı." });

        var success = await _mediator.Send(new UpdateSupportRequestStatusCommand(id, status), cancellationToken);
        if (!success) return NotFound();

        return NoContent();
    }
}

public record UpdateStatusRequest(string Status);
