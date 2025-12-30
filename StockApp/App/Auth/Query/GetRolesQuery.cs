using MediatR;
using StockApp.Common.Models;

namespace StockApp.App.Auth.Query;

public class GetRolesQuery : IRequest<List<RoleDto>>
{
}

