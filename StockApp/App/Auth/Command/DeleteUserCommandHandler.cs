using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using StockApp.Entities;
using StockApp.Hub;

namespace StockApp.App.Auth.Command;

public class DeleteUserCommandHandler : IRequestHandler<DeleteUserCommand>
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

        // Check if user is trying to delete themselves (optional safety check)
        // This can be added if needed

        var deletedUserId = user.Id;
        var result = await _userManager.DeleteAsync(user);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"Failed to delete user: {errors}");
        }

        // SignalR ile silinen kullanıcı ID'sini tüm client'lara gönder
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

