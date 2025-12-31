using MediatR;
using StockApp.Common.Models;

namespace StockApp.App.Auth.Command;

public class CreateRoleCommand : IRequest<RoleDto>
{
    public CreateRoleRequest Request { get; set; } = null!;
}


