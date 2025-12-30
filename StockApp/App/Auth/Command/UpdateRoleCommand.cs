using MediatR;
using StockApp.Common.Models;

namespace StockApp.App.Auth.Command;

public class UpdateRoleCommand : IRequest<RoleDto>
{
    public UpdateRoleRequest Request { get; set; } = null!;
}

