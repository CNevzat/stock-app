using MediatR;
using Microsoft.AspNetCore.Identity;
using StockApp.Entities;

namespace StockApp.App.Auth.Command;

public sealed record ForceChangePasswordCommand(string UserId, string NewPassword) : IRequest;

internal sealed class ForceChangePasswordCommandHandler : IRequestHandler<ForceChangePasswordCommand>
{
    private readonly UserManager<ApplicationUser> _userManager;

    public ForceChangePasswordCommandHandler(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task Handle(ForceChangePasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(request.UserId);
        if (user == null)
        {
            throw new KeyNotFoundException("User not found");
        }

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);

        var result = await _userManager.ResetPasswordAsync(user, token, request.NewPassword);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"Password reset failed: {errors}");
        }

        user.MustChangePassword = false;
        user.UpdatedAt = DateTime.UtcNow;
        await _userManager.UpdateAsync(user);
    }
}
