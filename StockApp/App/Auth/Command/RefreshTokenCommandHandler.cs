using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using StockApp.Common.Models;
using StockApp.Entities;
using StockApp.Services;

namespace StockApp.App.Auth.Command;

public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, AuthResponse>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly RoleManager<IdentityRole> _roleManager;

    public RefreshTokenCommandHandler(
        UserManager<ApplicationUser> userManager,
        IJwtTokenService jwtTokenService,
        RoleManager<IdentityRole> roleManager)
    {
        _userManager = userManager;
        _jwtTokenService = jwtTokenService;
        _roleManager = roleManager;
    }

    public async Task<AuthResponse> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var refreshTokenRequest = request.Request;

        var user = await _userManager.Users
            .FirstOrDefaultAsync(u => u.RefreshToken == refreshTokenRequest.RefreshToken, cancellationToken);

        if (user == null || !user.IsActive || 
            user.RefreshTokenExpiryTime == null || 
            user.RefreshTokenExpiryTime <= DateTime.UtcNow)
        {
            throw new UnauthorizedAccessException("Invalid or expired refresh token");
        }

        var roles = await _userManager.GetRolesAsync(user);
        
        // Get all user claims (both direct user claims and role claims)
        var userClaims = await _userManager.GetClaimsAsync(user);
        
        // Get role claims
        var roleClaims = new List<System.Security.Claims.Claim>();
        foreach (var roleName in roles)
        {
            var role = await _roleManager.FindByNameAsync(roleName);
            if (role != null)
            {
                var claims = await _roleManager.GetClaimsAsync(role);
                roleClaims.AddRange(claims);
            }
        }
        
        // Combine user claims and role claims (avoid duplicates)
        var allClaims = new List<System.Security.Claims.Claim>();
        var addedClaims = new HashSet<string>(); // Track added claims by "Type:Value"
        
        foreach (var claim in userClaims)
        {
            var key = $"{claim.Type}:{claim.Value}";
            if (!addedClaims.Contains(key))
            {
                allClaims.Add(claim);
                addedClaims.Add(key);
            }
        }
        
        foreach (var claim in roleClaims)
        {
            var key = $"{claim.Type}:{claim.Value}";
            if (!addedClaims.Contains(key))
            {
                allClaims.Add(claim);
                addedClaims.Add(key);
            }
        }
        
        var accessToken = _jwtTokenService.GenerateAccessToken(user, roles, allClaims);
        var refreshToken = _jwtTokenService.GenerateRefreshToken();
        var refreshTokenExpiration = _jwtTokenService.GetRefreshTokenExpiration();

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryTime = refreshTokenExpiration;
        await _userManager.UpdateAsync(user);

        return new AuthResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresAt = refreshTokenExpiration,
            User = new UserDto
            {
                Id = user.Id,
                Email = user.Email ?? string.Empty,
                UserName = user.UserName ?? string.Empty,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Roles = roles.ToList()
            }
        };
    }
}

