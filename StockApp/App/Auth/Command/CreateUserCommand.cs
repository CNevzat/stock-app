using MediatR;
using StockApp.Common.Models;

namespace StockApp.App.Auth.Command;

public class CreateUserCommand : IRequest<UserDto>
{
    public CreateUserRequest Request { get; set; } = null!;
}

