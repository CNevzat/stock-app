using MediatR;
using Microsoft.AspNetCore.Identity;
using StockApp.Common.Models;
using StockApp.Entities;
using StockApp.Services;

namespace StockApp.App.Auth.Command;

public class LoginCommandHandler : IRequestHandler<LoginCommand, AuthResponse>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly RoleManager<IdentityRole> _roleManager;

    public LoginCommandHandler(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IJwtTokenService jwtTokenService,
        RoleManager<IdentityRole> roleManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _jwtTokenService = jwtTokenService;
        _roleManager = roleManager;
    }

    public async Task<AuthResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var loginRequest = request.Request;

        var user = await _userManager.FindByEmailAsync(loginRequest.Email);
        if (user == null || !user.IsActive)
        {
            throw new UnauthorizedAccessException("Invalid email or password");
        }

        var result = await _signInManager.CheckPasswordSignInAsync(user, loginRequest.Password, lockoutOnFailure: true);
        if (!result.Succeeded)
        {
            throw new UnauthorizedAccessException("Invalid email or password");
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

        // Store refresh token (basit bir yaklaşım - production'da ayrı bir tablo kullanılabilir)
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
                Roles = roles.ToList(),
                MustChangePassword = user.MustChangePassword // Frontend will check this
            }
        };
    }
}

