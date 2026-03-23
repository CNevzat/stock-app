using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using StockApp.Entities;
using StockApp.Hub;

namespace StockApp.App.Auth.Command;

public sealed record DeleteUserCommand(string UserId) : IRequest;

internal sealed class DeleteUserCommandHandler : IRequestHandler<DeleteUserCommand>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IHubContext<StockHub> _hubContext;

    public DeleteUserCommandHandler(UserManager<ApplicationUser> userManager, IHubContext<StockHub> hubContext)
    {
        _userManager = userManager;
        _hubContext = hubContext;
    }

    public async Task Handle(DeleteUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(request.UserId);
        if (user == null)
        {
            throw new KeyNotFoundException("User not found");
        }

        var deletedUserId = user.Id;
        var result = await _userManager.DeleteAsync(user);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"Failed to delete user: {errors}");
        }

        try
        {
            await _hubContext.Clients.All.SendAsync("UserDeleted", deletedUserId, cancellationToken);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"SignalR user deleted gönderim hatası: {ex.Message}");
        }
    }
}
