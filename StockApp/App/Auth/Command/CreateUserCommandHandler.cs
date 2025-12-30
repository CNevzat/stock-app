using MediatR;
using Microsoft.AspNetCore.Identity;
using StockApp.Common.Models;
using StockApp.Entities;

namespace StockApp.App.Auth.Command;

public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, UserDto>
{
    private readonly UserManager<ApplicationUser> _userManager;

    public CreateUserCommandHandler(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
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

        return new UserDto
        {
            Id = user.Id,
            Email = user.Email ?? string.Empty,
            UserName = user.UserName ?? string.Empty,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Roles = roles.ToList()
        };
    }
}

