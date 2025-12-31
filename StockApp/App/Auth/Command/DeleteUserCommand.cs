using MediatR;

namespace StockApp.App.Auth.Command;

public class DeleteUserCommand : IRequest
{
    public string UserId { get; set; } = string.Empty;
}


