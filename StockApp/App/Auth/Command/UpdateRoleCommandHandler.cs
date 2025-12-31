using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using StockApp.Common.Models;
using StockApp.Services;
using StockApp.Hub;

namespace StockApp.App.Auth.Command;

public class UpdateRoleCommandHandler : IRequestHandler<UpdateRoleCommand, RoleDto>
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

        // Admin rolü: Ad değiştirilemez, yetkiler düzenlenemez (tüm yetkiler otomatik)
        if (role.Name == "Admin")
        {
            // Admin rolünün adı değiştirilemez
            if (!string.IsNullOrEmpty(roleRequest.Name) && roleRequest.Name != role.Name)
            {
                throw new InvalidOperationException("Admin rolünün adı değiştirilemez");
            }
            
            // Admin rolünün yetkileri düzenlenemez (tüm yetkiler otomatik)
            if (roleRequest.Claims != null)
            {
                throw new InvalidOperationException("Admin rolünün yetkileri düzenlenemez. Admin rolü tüm yetkilere otomatik sahiptir.");
            }
            
            // Admin rolü için tüm yetkileri garanti et
            await _rolePermissionService.EnsureAdminHasAllPermissionsAsync();
        }
        // User rolü: Ad değiştirilemez, ama yetkiler düzenlenebilir
        else if (role.Name == "User")
        {
            // User rolünün adı değiştirilemez
            if (!string.IsNullOrEmpty(roleRequest.Name) && roleRequest.Name != role.Name)
            {
                throw new InvalidOperationException("User rolünün adı değiştirilemez");
            }

            // User rolünün yetkileri düzenlenebilir
            if (roleRequest.Claims != null)
            {
                // Remove all existing claims
                var existingClaims = await _roleManager.GetClaimsAsync(role);
                foreach (var claim in existingClaims)
                {
                    await _roleManager.RemoveClaimAsync(role, claim);
                }

                // Add new claims
                foreach (var claim in roleRequest.Claims)
                {
                    var identityClaim = new System.Security.Claims.Claim(claim.Type, claim.Value);
                    await _roleManager.AddClaimAsync(role, identityClaim);
                }
            }
        }
        // Diğer roller: Hem ad hem yetkiler düzenlenebilir
        else
        {
            // Update role name if provided
            if (!string.IsNullOrEmpty(roleRequest.Name) && roleRequest.Name != role.Name)
            {
                // Check if new name already exists
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

            // Update claims if provided
            if (roleRequest.Claims != null)
            {
                // Remove all existing claims
                var existingClaims = await _roleManager.GetClaimsAsync(role);
                foreach (var claim in existingClaims)
                {
                    await _roleManager.RemoveClaimAsync(role, claim);
                }

                // Add new claims
                foreach (var claim in roleRequest.Claims)
                {
                    var identityClaim = new System.Security.Claims.Claim(claim.Type, claim.Value);
                    await _roleManager.AddClaimAsync(role, identityClaim);
                    
                    // Yeni yetki eklendiğinde Admin rolüne de ekle
                    if (claim.Type == "Permission")
                    {
                        await _rolePermissionService.AddPermissionToAdminAsync(claim.Value);
                    }
                }
            }
        }

        // Get updated role with claims
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

        // SignalR ile güncellenmiş rolü tüm client'lara gönder
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

