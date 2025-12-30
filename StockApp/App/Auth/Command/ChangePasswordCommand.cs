using MediatR;

namespace StockApp.App.Auth.Command;

public class ChangePasswordCommand : IRequest
{
    public string UserId { get; set; } = string.Empty;
    public string CurrentPassword { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}

