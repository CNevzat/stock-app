using MediatR;

namespace StockApp.App.Auth.Command;

public class AssignClaimCommand : IRequest
{
    public string UserId { get; set; } = string.Empty;
    public string ClaimType { get; set; } = string.Empty;
    public string ClaimValue { get; set; } = string.Empty;
}


