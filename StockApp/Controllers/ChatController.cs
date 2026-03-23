using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StockApp.App.Chat;
using StockApp.App.Chat.Query;

namespace StockApp.Controllers;

[ApiController]
[Route("api/chat")]
[Tags("Chat")]
public class ChatController : ControllerBase
{
    private readonly IMediator _mediator;

    public ChatController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("ask")]
    [Authorize(Policy = "CanUseChat")]
    public async Task<IActionResult> Ask([FromBody] ChatAskRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Question))
        {
            return BadRequest(new { message = "question alanı zorunludur." });
        }

        var response = await _mediator.Send(new GetChatResponseQuery(request.Question), cancellationToken);
        return Ok(response);
    }
}

public record ChatAskRequest(string Question);
