using MediatR;
using Microsoft.AspNetCore.Identity;
using StockApp.Entities;

namespace StockApp.App.Auth.Command;

public class RemoveClaimCommandHandler : IRequestHandler<RemoveClaimCommand>
{
    private readonly UserManager<ApplicationUser> _userManager;

    public RemoveClaimCommandHandler(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task Handle(RemoveClaimCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(request.UserId);
        if (user == null)
        {
            throw new KeyNotFoundException("User not found");
        }

        var claims = await _userManager.GetClaimsAsync(user);
        var claimToRemove = claims.FirstOrDefault(c => c.Type == request.ClaimType);
        
        if (claimToRemove == null)
        {
            throw new KeyNotFoundException($"Claim '{request.ClaimType}' not found for user");
        }

        var result = await _userManager.RemoveClaimAsync(user, claimToRemove);
        
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"Failed to remove claim: {errors}");
        }
    }
}


