using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using StockApp.Common.Models;
using StockApp.Hub;
using StockApp.Services;

namespace StockApp.App.Auth.Command;

public sealed record CreateRoleCommand(CreateRoleRequest Request) : IRequest<RoleDto>;

internal sealed class CreateRoleCommandHandler : IRequestHandler<CreateRoleCommand, RoleDto>
{
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly RolePermissionService _rolePermissionService;
    private readonly IHubContext<StockHub> _hubContext;

    public CreateRoleCommandHandler(RoleManager<IdentityRole> roleManager, RolePermissionService rolePermissionService, IHubContext<StockHub> hubContext)
    {
        _roleManager = roleManager;
        _rolePermissionService = rolePermissionService;
        _hubContext = hubContext;
    }

    public async Task<RoleDto> Handle(CreateRoleCommand request, CancellationToken cancellationToken)
    {
        var roleRequest = request.Request;

        if (await _roleManager.RoleExistsAsync(roleRequest.Name))
        {
            throw new InvalidOperationException($"Role '{roleRequest.Name}' already exists");
        }

        var role = new IdentityRole(roleRequest.Name);
        var result = await _roleManager.CreateAsync(role);

        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"Failed to create role: {errors}");
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

        var allClaims = await _roleManager.GetClaimsAsync(role);

        var roleDto = new RoleDto
        {
            Id = role.Id,
            Name = role.Name ?? string.Empty,
            Claims = allClaims.Select(c => new ClaimDto
            {
                Type = c.Type,
                Value = c.Value
            }).ToList()
        };

        try
        {
            await _hubContext.Clients.All.SendAsync("RoleCreated", roleDto, cancellationToken);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"SignalR role created gönderim hatası: {ex.Message}");
        }

        return roleDto;
    }
}
