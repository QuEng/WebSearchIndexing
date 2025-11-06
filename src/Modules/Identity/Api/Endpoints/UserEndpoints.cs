using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using WebSearchIndexing.Modules.Identity.Application.Abstractions;
using WebSearchIndexing.Modules.Identity.Application.Services;

namespace WebSearchIndexing.Modules.Identity.Api.Endpoints;

internal static class UserEndpoints
{
    public static RouteGroupBuilder MapUserEndpoints(this RouteGroupBuilder identityGroup)
    {
        ArgumentNullException.ThrowIfNull(identityGroup);

        var userGroup = identityGroup.MapGroup("/users").RequireAuthorization();

        userGroup.MapGet("/profile", HandleGetProfile);
        userGroup.MapPut("/profile", HandleUpdateProfile);
        userGroup.MapPost("/change-password", HandleChangePassword);

        return identityGroup;
    }

    private static async Task<IResult> HandleGetProfile(
        HttpContext context,
        IAuthService authService,
        CancellationToken cancellationToken)
    {
        try
        {
            var userId = context.User.FindFirst("user_id")?.Value;
            if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userGuid))
            {
                return Results.Unauthorized();
            }

            var result = await authService.GetUserProfileAsync(userGuid, cancellationToken);

            if (result.Success && result.Profile != null)
            {
                return Results.Ok(new UserProfileResponse(
                    result.Profile.Id,
                    result.Profile.Email,
                    result.Profile.FirstName,
                    result.Profile.LastName,
                    result.Profile.CreatedAt,
                    result.Profile.IsEmailVerified,
                    result.Profile.IsActive));
            }

            return Results.BadRequest(new { message = result.Message });
        }
        catch (Exception ex)
        {
            return Results.Problem($"Failed to get user profile: {ex.Message}");
        }
    }

    private static async Task<IResult> HandleUpdateProfile(
        HttpContext context,
        IAuthService authService,
        CancellationToken cancellationToken)
    {
        try
        {
            var userId = context.User.FindFirst("user_id")?.Value;
            if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userGuid))
            {
                return Results.Unauthorized();
            }

            var request = await context.Request.ReadFromJsonAsync<UpdateProfileRequest>(cancellationToken);
            if (request == null)
            {
                return Results.BadRequest(new { message = "Invalid request body" });
            }

            var updateRequest = new Application.Services.UpdateProfileRequest(request.FirstName, request.LastName);
            var result = await authService.UpdateUserProfileAsync(userGuid, updateRequest, cancellationToken);

            if (result.Success)
            {
                return Results.Ok(new { message = result.Message });
            }

            return Results.BadRequest(new { message = result.Message });
        }
        catch (Exception ex)
        {
            return Results.Problem($"Failed to update user profile: {ex.Message}");
        }
    }

    private static async Task<IResult> HandleChangePassword(
        HttpContext context,
        IAuthService authService,
        CancellationToken cancellationToken)
    {
        try
        {
            var userId = context.User.FindFirst("user_id")?.Value;
            if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userGuid))
            {
                return Results.Unauthorized();
            }

            var request = await context.Request.ReadFromJsonAsync<ChangePasswordRequest>(cancellationToken);
            if (request == null)
            {
                return Results.BadRequest(new { message = "Invalid request body" });
            }

            var changePasswordRequest = new Application.Services.ChangePasswordRequest(request.CurrentPassword, request.NewPassword);
            var result = await authService.ChangePasswordAsync(userGuid, changePasswordRequest, cancellationToken);

            if (result.Success)
            {
                return Results.Ok(new { message = result.Message });
            }

            return Results.BadRequest(new { message = result.Message });
        }
        catch (Exception ex)
        {
            return Results.Problem($"Failed to change password: {ex.Message}");
        }
    }
}

// Request models
public record UpdateProfileRequest(string FirstName, string LastName);
public record ChangePasswordRequest(string CurrentPassword, string NewPassword);
public record UserProfileResponse(
    Guid Id, 
    string Email, 
    string FirstName, 
    string LastName, 
    DateTime CreatedAt,
    bool IsEmailVerified,
    bool IsActive);
