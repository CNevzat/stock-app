using MediatR;

namespace StockApp.App.Auth.Command;

public class RemoveClaimCommand : IRequest
{
    public string UserId { get; set; } = string.Empty;
    public string ClaimType { get; set; } = string.Empty;
}


