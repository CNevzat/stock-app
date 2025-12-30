using StockApp.Entities;

namespace StockApp.Services;

public interface IJwtTokenService
{
    string GenerateAccessToken(ApplicationUser user, IList<string> roles);
    string GenerateRefreshToken();
    DateTime GetRefreshTokenExpiration();
}

