using MediatR;
using Microsoft.AspNetCore.Identity;
using StockApp.Entities;

namespace StockApp.App.Auth.Command;

public class ChangePasswordCommandHandler : IRequestHandler<ChangePasswordCommand>
{
    private readonly UserManager<ApplicationUser> _userManager;

    public ChangePasswordCommandHandler(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(request.UserId);
        if (user == null)
        {
            throw new KeyNotFoundException("User not found");
        }

        // Verify current password
        var passwordValid = await _userManager.CheckPasswordAsync(user, request.CurrentPassword);
        if (!passwordValid)
        {
            throw new UnauthorizedAccessException("Current password is incorrect");
        }

        // Change password (Identity automatically hashes the new password)
        var result = await _userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"Password change failed: {errors}");
        }

        // Clear MustChangePassword flag
        user.MustChangePassword = false;
        user.UpdatedAt = DateTime.UtcNow;
        await _userManager.UpdateAsync(user);
    }
}


