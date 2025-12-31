using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using StockApp.Hub;

namespace StockApp.App.Auth.Command;

public class DeleteRoleCommandHandler : IRequestHandler<DeleteRoleCommand>
{
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly UserManager<StockApp.Entities.ApplicationUser> _userManager;
    private readonly IHubContext<StockHub> _hubContext;

    public DeleteRoleCommandHandler(
        RoleManager<IdentityRole> roleManager,
        UserManager<StockApp.Entities.ApplicationUser> userManager,
        IHubContext<StockHub> hubContext)
    {
        _roleManager = roleManager;
        _userManager = userManager;
        _hubContext = hubContext;
    }

    public async Task Handle(DeleteRoleCommand request, CancellationToken cancellationToken)
    {
        var role = await _roleManager.FindByIdAsync(request.RoleId);
        if (role == null)
        {
            throw new KeyNotFoundException("Role not found");
        }

        // Check if role is protected (User or Admin)
        if (role.Name == "User" || role.Name == "Admin")
        {
            throw new InvalidOperationException($"Role '{role.Name}' cannot be deleted");
        }

        // Get all users with this role
        var usersInRole = await _userManager.GetUsersInRoleAsync(role.Name ?? string.Empty);

        // Assign "User" role to all users who had the deleted role
        var defaultRole = await _roleManager.FindByNameAsync("User");
        if (defaultRole == null)
        {
            throw new InvalidOperationException("Default 'User' role not found");
        }

        foreach (var user in usersInRole)
        {
            // Remove old role
            await _userManager.RemoveFromRoleAsync(user, role.Name ?? string.Empty);
            
            // Add User role if not already has it
            if (!await _userManager.IsInRoleAsync(user, "User"))
            {
                await _userManager.AddToRoleAsync(user, "User");
            }
        }

        // Delete the role
        var deletedRoleId = role.Id;
        var result = await _roleManager.DeleteAsync(role);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"Failed to delete role: {errors}");
        }

        // SignalR ile silinen rol ID'sini tüm client'lara gönder
        try
        {
            await _hubContext.Clients.All.SendAsync("RoleDeleted", deletedRoleId, cancellationToken);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"SignalR role deleted gönderim hatası: {ex.Message}");
        }
    }
}

