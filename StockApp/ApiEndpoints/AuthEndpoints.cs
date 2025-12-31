using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StockApp.App.Auth.Command;
using StockApp.App.Auth.Query;
using StockApp.Common.Models;

namespace StockApp.ApiEndpoints;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/auth").WithTags("Auth");

        // Public endpoints
        group.MapPost("/login", Login)
            .WithName("Login")
            .WithSummary("User login")
            .Accepts<LoginRequest>("application/json")
            .Produces<AuthResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized);

        group.MapPost("/refresh-token", RefreshToken)
            .WithName("RefreshToken")
            .WithSummary("Refresh access token")
            .Accepts<RefreshTokenRequest>("application/json")
            .Produces<AuthResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized);

        // Protected endpoints
        group.MapGet("/users", GetUsers)
            .WithName("GetUsers")
            .WithSummary("Get all users")
            .RequireAuthorization("AdminOnly")
            .Produces<List<UserListDto>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden);

        group.MapGet("/me", GetCurrentUser)
            .WithName("GetCurrentUser")
            .WithSummary("Get current user information")
            .RequireAuthorization()
            .Produces<UserDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized);

        // User management endpoints (Protected)
        group.MapPost("/users", CreateUser)
            .WithName("CreateUser")
            .WithSummary("Create a new user (Admin/Manager only)")
            .RequireAuthorization("CanCreateUser")
            .Accepts<CreateUserRequest>("application/json")
            .Produces<UserDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden);

        group.MapPut("/users", UpdateUser)
            .WithName("UpdateUser")
            .WithSummary("Update a user (Admin/Manager only)")
            .RequireAuthorization("CanManageUsers")
            .Accepts<UpdateUserRequest>("application/json")
            .Produces<UserListDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden);

        group.MapDelete("/users/{userId}", DeleteUser)
            .WithName("DeleteUser")
            .WithSummary("Delete a user (Admin/Manager only)")
            .RequireAuthorization("CanManageUsers")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden);

        group.MapPost("/change-password", ChangePassword)
            .WithName("ChangePassword")
            .WithSummary("Change current user's password")
            .RequireAuthorization()
            .Accepts<ChangePasswordRequest>("application/json")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized);

        group.MapPost("/force-change-password", ForceChangePassword)
            .WithName("ForceChangePassword")
            .WithSummary("Force change password (for first login)")
            .RequireAuthorization()
            .Accepts<ForceChangePasswordRequest>("application/json")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized);

        // Claim management endpoints
        group.MapPost("/users/claims", AssignClaim)
            .WithName("AssignClaim")
            .WithSummary("Assign a claim to a user")
            .RequireAuthorization("CanManageUsers")
            .Accepts<AssignClaimRequest>("application/json")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden);

        group.MapDelete("/users/claims", RemoveClaim)
            .WithName("RemoveClaim")
            .WithSummary("Remove a claim from a user")
            .RequireAuthorization("CanManageUsers")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden);

        // Role management endpoints
        group.MapGet("/roles", GetRoles)
            .WithName("GetRoles")
            .WithSummary("Get all roles")
            .RequireAuthorization("CanViewRoles")
            .Produces<List<RoleDto>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden);

        group.MapPost("/roles", CreateRole)
            .WithName("CreateRole")
            .WithSummary("Create a new role")
            .RequireAuthorization("CanManageRoles")
            .Accepts<CreateRoleRequest>("application/json")
            .Produces<RoleDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden);

        group.MapPut("/roles", UpdateRole)
            .WithName("UpdateRole")
            .WithSummary("Update a role")
            .RequireAuthorization("CanManageRoles")
            .Accepts<UpdateRoleRequest>("application/json")
            .Produces<RoleDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden);

        group.MapDelete("/roles/{roleId}", DeleteRole)
            .WithName("DeleteRole")
            .WithSummary("Delete a role")
            .RequireAuthorization("CanManageRoles")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden);
    }

    private static async Task<IResult> Login(
        [FromBody] LoginRequest request,
        IMediator mediator)
    {
        try
        {
            var command = new LoginCommand { Request = request };
            var result = await mediator.Send(command);
            return Results.Ok(result);
        }
        catch (UnauthorizedAccessException)
        {
            return Results.Unauthorized();
        }
        catch (Exception ex)
        {
            return Results.BadRequest(new { message = ex.Message });
        }
    }

    private static async Task<IResult> RefreshToken(
        [FromBody] RefreshTokenRequest request,
        IMediator mediator)
    {
        try
        {
            var command = new RefreshTokenCommand { Request = request };
            var result = await mediator.Send(command);
            return Results.Ok(result);
        }
        catch (UnauthorizedAccessException)
        {
            return Results.Unauthorized();
        }
        catch (Exception ex)
        {
            return Results.BadRequest(new { message = ex.Message });
        }
    }

    private static async Task<IResult> GetUsers(
        IMediator mediator)
    {
        try
        {
            var query = new GetUsersQuery();
            var result = await mediator.Send(query);
            return Results.Ok(result);
        }
        catch (Exception ex)
        {
            return Results.BadRequest(new { message = ex.Message });
        }
    }

    private static async Task<IResult> GetCurrentUser(
        HttpContext httpContext,
        IMediator mediator)
    {
        try
        {
            var userId = httpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Results.Unauthorized();
            }

            // Get user from database
            var usersQuery = new GetUsersQuery();
            var users = await mediator.Send(usersQuery);
            var user = users.FirstOrDefault(u => u.Id == userId);

            if (user == null)
            {
                return Results.NotFound();
            }

            var userDto = new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                UserName = user.UserName,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Roles = user.Roles,
                MustChangePassword = user.MustChangePassword
            };

            return Results.Ok(userDto);
        }
        catch (Exception ex)
        {
            return Results.BadRequest(new { message = ex.Message });
        }
    }

    private static async Task<IResult> CreateUser(
        [FromBody] CreateUserRequest request,
        IMediator mediator)
    {
        try
        {
            var command = new CreateUserCommand { Request = request };
            var result = await mediator.Send(command);
            return Results.Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return Results.BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return Results.BadRequest(new { message = ex.Message });
        }
    }

    private static async Task<IResult> ChangePassword(
        [FromBody] ChangePasswordRequest request,
        HttpContext httpContext,
        IMediator mediator)
    {
        try
        {
            var userId = httpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Results.Unauthorized();
            }

            if (request.NewPassword != request.ConfirmPassword)
            {
                return Results.BadRequest(new { message = "New password and confirm password do not match" });
            }

            var command = new ChangePasswordCommand
            {
                UserId = userId,
                CurrentPassword = request.CurrentPassword,
                NewPassword = request.NewPassword
            };
            await mediator.Send(command);
            return Results.Ok(new { message = "Password changed successfully" });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Results.BadRequest(new { message = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            return Results.NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Results.BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return Results.BadRequest(new { message = ex.Message });
        }
    }

    private static async Task<IResult> ForceChangePassword(
        [FromBody] ForceChangePasswordRequest request,
        HttpContext httpContext,
        IMediator mediator)
    {
        try
        {
            var userId = httpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Results.Unauthorized();
            }

            if (request.NewPassword != request.ConfirmPassword)
            {
                return Results.BadRequest(new { message = "New password and confirm password do not match" });
            }

            var command = new ForceChangePasswordCommand
            {
                UserId = userId,
                NewPassword = request.NewPassword
            };
            await mediator.Send(command);
            return Results.Ok(new { message = "Password changed successfully" });
        }
        catch (KeyNotFoundException ex)
        {
            return Results.NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Results.BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return Results.BadRequest(new { message = ex.Message });
        }
    }

    private static async Task<IResult> AssignClaim(
        [FromBody] AssignClaimRequest request,
        IMediator mediator)
    {
        try
        {
            var command = new AssignClaimCommand
            {
                UserId = request.UserId,
                ClaimType = request.ClaimType,
                ClaimValue = request.ClaimValue
            };
            await mediator.Send(command);
            return Results.Ok(new { message = "Claim assigned successfully" });
        }
        catch (KeyNotFoundException ex)
        {
            return Results.NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Results.BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return Results.BadRequest(new { message = ex.Message });
        }
    }

    private static async Task<IResult> RemoveClaim(
        [FromQuery] string userId,
        [FromQuery] string claimType,
        IMediator mediator)
    {
        try
        {
            var command = new RemoveClaimCommand
            {
                UserId = userId,
                ClaimType = claimType
            };
            await mediator.Send(command);
            return Results.Ok(new { message = "Claim removed successfully" });
        }
        catch (KeyNotFoundException ex)
        {
            return Results.NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Results.BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return Results.BadRequest(new { message = ex.Message });
        }
    }

    private static async Task<IResult> GetRoles(
        IMediator mediator)
    {
        try
        {
            var query = new GetRolesQuery();
            var result = await mediator.Send(query);
            return Results.Ok(result);
        }
        catch (Exception ex)
        {
            return Results.BadRequest(new { message = ex.Message });
        }
    }

    private static async Task<IResult> CreateRole(
        [FromBody] CreateRoleRequest request,
        IMediator mediator)
    {
        try
        {
            var command = new CreateRoleCommand { Request = request };
            var result = await mediator.Send(command);
            return Results.Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return Results.BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return Results.BadRequest(new { message = ex.Message });
        }
    }

    private static async Task<IResult> UpdateRole(
        [FromBody] UpdateRoleRequest request,
        IMediator mediator)
    {
        try
        {
            var command = new UpdateRoleCommand { Request = request };
            var result = await mediator.Send(command);
            return Results.Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return Results.NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Results.BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return Results.BadRequest(new { message = ex.Message });
        }
    }

    private static async Task<IResult> DeleteRole(
        string roleId,
        IMediator mediator)
    {
        try
        {
            var command = new DeleteRoleCommand { RoleId = roleId };
            await mediator.Send(command);
            return Results.Ok(new { message = "Role deleted successfully" });
        }
        catch (KeyNotFoundException ex)
        {
            return Results.NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Results.BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return Results.BadRequest(new { message = ex.Message });
        }
    }

    private static async Task<IResult> UpdateUser(
        [FromBody] UpdateUserRequest request,
        IMediator mediator)
    {
        try
        {
            var command = new UpdateUserCommand { Request = request };
            var result = await mediator.Send(command);
            return Results.Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return Results.NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Results.BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return Results.BadRequest(new { message = ex.Message });
        }
    }

    private static async Task<IResult> DeleteUser(
        string userId,
        IMediator mediator)
    {
        try
        {
            var command = new DeleteUserCommand { UserId = userId };
            await mediator.Send(command);
            return Results.Ok(new { message = "User deleted successfully" });
        }
        catch (KeyNotFoundException ex)
        {
            return Results.NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Results.BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return Results.BadRequest(new { message = ex.Message });
        }
    }
}

