using MediatR;

namespace StockApp.App.Auth.Command;

public class ForceChangePasswordCommand : IRequest
{
    public string UserId { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}


