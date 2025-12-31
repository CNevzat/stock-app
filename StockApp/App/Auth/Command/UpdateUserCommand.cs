using MediatR;
using StockApp.Common.Models;

namespace StockApp.App.Auth.Command;

public class UpdateUserCommand : IRequest<UserListDto>
{
    public UpdateUserRequest Request { get; set; } = null!;
}


