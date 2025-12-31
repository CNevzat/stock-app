using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using StockApp.Common.Models;
using StockApp.Entities;
using StockApp.Hub;

namespace StockApp.App.Auth.Command;

public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, UserDto>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IHubContext<StockHub> _hubContext;

    public CreateUserCommandHandler(UserManager<ApplicationUser> userManager, IHubContext<StockHub> hubContext)
    {
        _userManager = userManager;
        _hubContext = hubContext;
    }

    public async Task<UserDto> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        var createRequest = request.Request;

        // Check if user already exists
        var existingUser = await _userManager.FindByEmailAsync(createRequest.Email);
        if (existingUser != null)
        {
            throw new InvalidOperationException("User with this email already exists");
        }

        var user = new ApplicationUser
        {
            UserName = createRequest.UserName ?? createRequest.Email,
            Email = createRequest.Email,
            FirstName = createRequest.FirstName,
            LastName = createRequest.LastName,
            IsActive = true,
            MustChangePassword = createRequest.MustChangePassword,
            CreatedAt = DateTime.UtcNow
        };

        // Create user with password (Identity automatically hashes it)
        var result = await _userManager.CreateAsync(user, createRequest.Password);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"User creation failed: {errors}");
        }

        // Assign role (only one role allowed)
        var validRoles = new[] { "Admin", "Manager", "User" };
        var roleToAssign = !string.IsNullOrEmpty(createRequest.Role) && validRoles.Contains(createRequest.Role)
            ? createRequest.Role
            : "User"; // Default role: User

        await _userManager.AddToRoleAsync(user, roleToAssign);

        var roles = await _userManager.GetRolesAsync(user);
        var claims = await _userManager.GetClaimsAsync(user);

        var userDto = new UserDto
        {
            Id = user.Id,
            Email = user.Email ?? string.Empty,
            UserName = user.UserName ?? string.Empty,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Roles = roles.ToList()
        };

        // SignalR ile yeni kullanıcıyı tüm client'lara gönder
        try
        {
            var userListDto = new UserListDto
            {
                Id = user.Id,
                Email = user.Email ?? string.Empty,
                UserName = user.UserName ?? string.Empty,
                FirstName = user.FirstName,
                LastName = user.LastName,
                IsActive = user.IsActive,
                MustChangePassword = user.MustChangePassword,
                CreatedAt = user.CreatedAt,
                Roles = roles.ToList(),
                Claims = claims.Select(c => $"{c.Type}:{c.Value}").ToList()
            };
            await _hubContext.Clients.All.SendAsync("UserCreated", userListDto, cancellationToken);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"SignalR user created gönderim hatası: {ex.Message}");
        }

        return userDto;
    }
}

