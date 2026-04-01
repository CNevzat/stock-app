using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StockApp.App.Chat.Query;
using StockApp.App.SupportRequest.Command;

namespace StockApp.Controllers;

[ApiController]
[Route("api/public-chat")]
[AllowAnonymous]
[Tags("PublicChat")]
public class PublicChatController : ControllerBase
{
    private readonly IMediator _mediator;

    public PublicChatController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("ask")]
    public async Task<IActionResult> Ask([FromBody] PublicChatAskRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Question))
            return BadRequest(new { message = "question alanı zorunludur." });

        var response = await _mediator.Send(new GetPublicChatResponseQuery(request.Question), cancellationToken);
        return Ok(response);
    }

    [HttpPost("support-request")]
    public async Task<IActionResult> CreateSupportRequest([FromBody] CreateSupportRequestRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Email))
            return BadRequest(new { message = "email alanı zorunludur." });

        if (string.IsNullOrWhiteSpace(request.Subject))
            return BadRequest(new { message = "subject alanı zorunludur." });

        if (!IsValidEmail(request.Email))
            return BadRequest(new { message = "Geçerli bir e-posta adresi giriniz." });

        var result = await _mediator.Send(
            new CreateSupportRequestCommand(request.Email, request.Subject, request.DetectedIntent),
            cancellationToken);

        return Ok(result);
    }

    private static bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email.Trim();
        }
        catch
        {
            return false;
        }
    }
}

public record PublicChatAskRequest(string Question);
public record CreateSupportRequestRequest(string Email, string Subject, string? DetectedIntent);
