using MediatR;
using StockApp.Common.Models;

namespace StockApp.App.Auth.Command;

public class RegisterCommand : IRequest<AuthResponse>
{
    public RegisterRequest Request { get; set; } = null!;
}

