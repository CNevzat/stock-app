using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using StockApp.Common.Models;
using StockApp.Entities;
using StockApp.Hub;

namespace StockApp.App.Auth.Command;

public class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand, UserListDto>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IHubContext<StockHub> _hubContext;

    public UpdateUserCommandHandler(UserManager<ApplicationUser> userManager, IHubContext<StockHub> hubContext)
    {
        _userManager = userManager;
        _hubContext = hubContext;
    }

    public async Task<UserListDto> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        var updateRequest = request.Request;

        var user = await _userManager.FindByIdAsync(updateRequest.Id);
        if (user == null)
        {
            throw new KeyNotFoundException("User not found");
        }

        // Update basic properties
        if (!string.IsNullOrEmpty(updateRequest.FirstName))
        {
            user.FirstName = updateRequest.FirstName;
        }

        if (!string.IsNullOrEmpty(updateRequest.LastName))
        {
            user.LastName = updateRequest.LastName;
        }

        if (!string.IsNullOrEmpty(updateRequest.Email))
        {
            user.Email = updateRequest.Email;
            user.UserName = updateRequest.Email; // Update username to match email
        }

        if (updateRequest.IsActive.HasValue)
        {
            user.IsActive = updateRequest.IsActive.Value;
        }

        user.UpdatedAt = DateTime.UtcNow;

        var updateResult = await _userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            var errors = string.Join(", ", updateResult.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"Failed to update user: {errors}");
        }

        // Update role if provided
        if (!string.IsNullOrEmpty(updateRequest.Role))
        {
            var validRoles = new[] { "Admin", "Manager", "User" };
            if (validRoles.Contains(updateRequest.Role))
            {
                // Get current roles
                var currentRoles = await _userManager.GetRolesAsync(user);
                
                // Remove all current roles
                if (currentRoles.Count > 0)
                {
                    await _userManager.RemoveFromRolesAsync(user, currentRoles);
                }

                // Add new role (only one role allowed)
                await _userManager.AddToRoleAsync(user, updateRequest.Role);
            }
        }

        // Get updated user with roles
        var updatedRoles = await _userManager.GetRolesAsync(user);
        var updatedClaims = await _userManager.GetClaimsAsync(user);

        var userListDto = new UserListDto
        {
            Id = user.Id,
            Email = user.Email ?? string.Empty,
            UserName = user.UserName ?? string.Empty,
            FirstName = user.FirstName,
            LastName = user.LastName,
            IsActive = user.IsActive,
            MustChangePassword = user.MustChangePassword,
            CreatedAt = user.CreatedAt,
            Roles = updatedRoles.ToList(),
            Claims = updatedClaims.Select(c => $"{c.Type}:{c.Value}").ToList()
        };

        // SignalR ile güncellenmiş kullanıcıyı tüm client'lara gönder
        try
        {
            await _hubContext.Clients.All.SendAsync("UserUpdated", userListDto, cancellationToken);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"SignalR user updated gönderim hatası: {ex.Message}");
        }

        return userListDto;
    }
}

