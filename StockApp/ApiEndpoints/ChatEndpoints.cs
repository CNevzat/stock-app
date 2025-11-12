using MediatR;
using StockApp.App.Chat;
using StockApp.App.Chat.Query;

namespace StockApp.ApiEndpoints;

public static class ChatEndpoints
{
    public static void MapChat(this WebApplication app)
    {
        var group = app.MapGroup("/api/chat").WithTags("Chat");

        group.MapPost("/ask", async (
            ChatAskRequest request,
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            if (string.IsNullOrWhiteSpace(request.Question))
            {
                return Results.BadRequest(new { message = "question alanı zorunludur." });
            }

            var response = await mediator.Send(new GetChatResponseQuery(request.Question), cancellationToken);
            return Results.Ok(response);
        })
        .Produces<ChatResponseDto>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest);
    }
}

public record ChatAskRequest(string Question);


