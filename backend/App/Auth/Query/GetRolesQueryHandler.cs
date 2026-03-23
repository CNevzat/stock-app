using MediatR;
using Microsoft.AspNetCore.Identity;
using StockApp.Common.Models;
using StockApp.Services;

namespace StockApp.App.Auth.Query;

public class GetRolesQueryHandler : IRequestHandler<GetRolesQuery, List<RoleDto>>
{
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly RolePermissionService _rolePermissionService;

    public GetRolesQueryHandler(RoleManager<IdentityRole> roleManager, RolePermissionService rolePermissionService)
    {
        _roleManager = roleManager;
        _rolePermissionService = rolePermissionService;
    }

    public async Task<List<RoleDto>> Handle(GetRolesQuery request, CancellationToken cancellationToken)
    {
        // Admin rolünün tüm yetkilere sahip olduğundan emin ol
        await _rolePermissionService.EnsureAdminHasAllPermissionsAsync();

        var roles = _roleManager.Roles.ToList();
        var roleDtos = new List<RoleDto>();

        foreach (var role in roles)
        {
            var claims = await _roleManager.GetClaimsAsync(role);
            
            // Admin rolü için tüm yetkileri göster
            if (role.Name == "Admin")
            {
                var allPermissions = RolePermissionService.GetAllPermissions();
                var adminClaims = allPermissions.Select(p => new ClaimDto
                {
                    Type = "Permission",
                    Value = p
                }).ToList();
                
                roleDtos.Add(new RoleDto
                {
                    Id = role.Id,
                    Name = role.Name ?? string.Empty,
                    Claims = adminClaims
                });
            }
            else
            {
                roleDtos.Add(new RoleDto
                {
                    Id = role.Id,
                    Name = role.Name ?? string.Empty,
                    Claims = claims.Select(c => new ClaimDto
                    {
                        Type = c.Type,
                        Value = c.Value
                    }).ToList()
                });
            }
        }

        return roleDtos;
    }
}

