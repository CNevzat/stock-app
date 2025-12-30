using MediatR;

namespace StockApp.App.Auth.Command;

public class DeleteRoleCommand : IRequest
{
    public string RoleId { get; set; } = string.Empty;
}

