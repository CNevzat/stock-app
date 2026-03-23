using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using StockApp.Common.Models;
using StockApp.Hub;
using StockApp.Services;

namespace StockApp.App.Auth.Command;

public sealed record UpdateRoleCommand(UpdateRoleRequest Request) : IRequest<RoleDto>;

internal sealed class UpdateRoleCommandHandler : IRequestHandler<UpdateRoleCommand, RoleDto>
{
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly RolePermissionService _rolePermissionService;
    private readonly IHubContext<StockHub> _hubContext;

    public UpdateRoleCommandHandler(RoleManager<IdentityRole> roleManager, RolePermissionService rolePermissionService, IHubContext<StockHub> hubContext)
    {
        _roleManager = roleManager;
        _rolePermissionService = rolePermissionService;
        _hubContext = hubContext;
    }

    public async Task<RoleDto> Handle(UpdateRoleCommand request, CancellationToken cancellationToken)
    {
        var roleRequest = request.Request;

        var role = await _roleManager.FindByIdAsync(roleRequest.Id);
        if (role == null)
        {
            throw new KeyNotFoundException("Role not found");
        }

        if (role.Name == "Admin")
        {
            if (!string.IsNullOrEmpty(roleRequest.Name) && roleRequest.Name != role.Name)
            {
                throw new InvalidOperationException("Admin rolünün adı değiştirilemez");
            }

            if (roleRequest.Claims != null)
            {
                throw new InvalidOperationException("Admin rolünün yetkileri düzenlenemez. Admin rolü tüm yetkilere otomatik sahiptir.");
            }

            await _rolePermissionService.EnsureAdminHasAllPermissionsAsync();
        }
        else if (role.Name == "User")
        {
            if (!string.IsNullOrEmpty(roleRequest.Name) && roleRequest.Name != role.Name)
            {
                throw new InvalidOperationException("User rolünün adı değiştirilemez");
            }

            if (roleRequest.Claims != null)
            {
                var existingClaims = await _roleManager.GetClaimsAsync(role);
                foreach (var claim in existingClaims)
                {
                    await _roleManager.RemoveClaimAsync(role, claim);
                }

                foreach (var claim in roleRequest.Claims)
                {
                    var identityClaim = new System.Security.Claims.Claim(claim.Type, claim.Value);
                    await _roleManager.AddClaimAsync(role, identityClaim);
                }
            }
        }
        else
        {
            if (!string.IsNullOrEmpty(roleRequest.Name) && roleRequest.Name != role.Name)
            {
                if (await _roleManager.RoleExistsAsync(roleRequest.Name))
                {
                    throw new InvalidOperationException($"Role '{roleRequest.Name}' already exists");
                }

                role.Name = roleRequest.Name;
                var result = await _roleManager.UpdateAsync(role);

                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    throw new InvalidOperationException($"Failed to update role: {errors}");
                }
            }

            if (roleRequest.Claims != null)
            {
                var existingClaims = await _roleManager.GetClaimsAsync(role);
                foreach (var claim in existingClaims)
                {
                    await _roleManager.RemoveClaimAsync(role, claim);
                }

                foreach (var claim in roleRequest.Claims)
                {
                    var identityClaim = new System.Security.Claims.Claim(claim.Type, claim.Value);
                    await _roleManager.AddClaimAsync(role, identityClaim);

                    if (claim.Type == "Permission")
                    {
                        await _rolePermissionService.AddPermissionToAdminAsync(claim.Value);
                    }
                }
            }
        }

        var updatedClaims = await _roleManager.GetClaimsAsync(role);

        var roleDto = new RoleDto
        {
            Id = role.Id,
            Name = role.Name ?? string.Empty,
            Claims = updatedClaims.Select(c => new ClaimDto
            {
                Type = c.Type,
                Value = c.Value
            }).ToList()
        };

        try
        {
            await _hubContext.Clients.All.SendAsync("RoleUpdated", roleDto, cancellationToken);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"SignalR role updated gönderim hatası: {ex.Message}");
        }

        return roleDto;
    }
}
