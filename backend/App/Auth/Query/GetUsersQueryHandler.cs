using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using StockApp.Common.Models;
using StockApp.Entities;

namespace StockApp.App.Auth.Query;

public class GetUsersQueryHandler : IRequestHandler<GetUsersQuery, List<UserListDto>>
{
    private readonly UserManager<ApplicationUser> _userManager;

    public GetUsersQueryHandler(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<List<UserListDto>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
    {
        var users = await _userManager.Users
            .OrderBy(u => u.CreatedAt)
            .ToListAsync(cancellationToken);

        var userDtos = new List<UserListDto>();

        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            var claims = await _userManager.GetClaimsAsync(user);
            userDtos.Add(new UserListDto
            {
                Id = user.Id,
                Email = user.Email ?? string.Empty,
                UserName = user.UserName ?? string.Empty,
                FirstName = user.FirstName,
                LastName = user.LastName,
                IsActive = user.IsActive,
                MustChangePassword = user.MustChangePassword,
                CreatedAt = user.CreatedAt,
                Roles = roles.ToList(),
                Claims = claims.Select(c => $"{c.Type}:{c.Value}").ToList()
            });
        }

        return userDtos;
    }
}

