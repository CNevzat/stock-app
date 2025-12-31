using MediatR;
using StockApp.Common.Models;

namespace StockApp.App.Auth.Command;

public class RefreshTokenCommand : IRequest<AuthResponse>
{
    public RefreshTokenRequest Request { get; set; } = null!;
}


