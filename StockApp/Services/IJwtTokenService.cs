using System.Security.Claims;
using StockApp.Entities;

namespace StockApp.Services;

public interface IJwtTokenService
{
    string GenerateAccessToken(ApplicationUser user, IList<string> roles, IList<Claim>? userClaims = null);
    string GenerateRefreshToken();
    DateTime GetRefreshTokenExpiration();
}

