using MediatR;
using StockApp.Common.Models;

namespace StockApp.App.Auth.Command;

public class LoginCommand : IRequest<AuthResponse>
{
    public LoginRequest Request { get; set; } = null!;
}

