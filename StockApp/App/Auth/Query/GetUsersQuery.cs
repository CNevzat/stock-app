using MediatR;
using StockApp.Common.Models;

namespace StockApp.App.Auth.Query;

public class GetUsersQuery : IRequest<List<UserListDto>>
{
    // Pagination ve filtreleme eklenebilir
}


