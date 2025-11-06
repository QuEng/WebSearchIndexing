using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using WebSearchIndexing.Modules.Identity.Application.Abstractions;
using WebSearchIndexing.Modules.Identity.Application.Services;

namespace WebSearchIndexing.Modules.Identity.Api.Endpoints;

internal static class AuthEndpoints
{
    public static RouteGroupBuilder MapAuthEndpoints(this RouteGroupBuilder identityGroup)
    {
        ArgumentNullException.ThrowIfNull(identityGroup);

        var authGroup = identityGroup.MapGroup("/auth");

        authGroup.MapPost("/login", HandleLogin);
        authGroup.MapPost("/register", HandleRegister);
        authGroup.MapPost("/refresh", HandleRefreshToken);
        authGroup.MapPost("/logout", HandleLogout);

        return identityGroup;
    }

    private static async Task<IResult> HandleLogin(
        HttpContext context,
        IAuthService authService,
        CancellationToken cancellationToken)
    {
        try
        {
            var request = await context.Request.ReadFromJsonAsync<LoginRequest>(cancellationToken);
            if (request == null)
            {
                return Results.BadRequest(new { message = "Invalid request body" });
            }

            var authContext = new AuthenticationContext(
                IpAddress: GetClientIpAddress(context),
                UserAgent: context.Request.Headers.UserAgent.ToString(),
                SetCookie: (name, value, options) => {
                    context.Response.Cookies.Append(name, value, new Microsoft.AspNetCore.Http.CookieOptions
                    {
                        HttpOnly = options.HttpOnly,
                        Secure = options.Secure,
                        SameSite = Enum.Parse<SameSiteMode>(options.SameSite),
                        Expires = options.Expires
                    });
                },
                DeleteCookie: (name) => context.Response.Cookies.Delete(name)
            );

            var result = await authService.LoginAsync(request.Email, request.Password, authContext, cancellationToken);

            if (result.Success)
            {
                return Results.Ok(new TokenResponse(
                    AccessToken: result.AccessToken,
                    ExpiresIn: result.ExpiresIn,
                    ExpiresAt: result.ExpiresAt));
            }

            return Results.BadRequest(new { message = result.Error });
        }
        catch (Exception ex)
        {
            return Results.Problem($"Login failed: {ex.Message}");
        }
    }

    private static async Task<IResult> HandleRefreshToken(
        HttpContext context,
        ITokenService tokenService,
        CancellationToken cancellationToken)
    {
        try
        {
            var refreshToken = context.Request.Cookies["refresh_token"];
            if (string.IsNullOrEmpty(refreshToken))
            {
                return Results.Unauthorized();
            }

            var authContext = new AuthenticationContext(
                IpAddress: GetClientIpAddress(context),
                UserAgent: context.Request.Headers.UserAgent.ToString(),
                SetCookie: (name, value, options) => {
                    context.Response.Cookies.Append(name, value, new Microsoft.AspNetCore.Http.CookieOptions
                    {
                        HttpOnly = options.HttpOnly,
                        Secure = options.Secure,
                        SameSite = Enum.Parse<SameSiteMode>(options.SameSite),
                        Expires = options.Expires
                    });
                },
                DeleteCookie: (name) => context.Response.Cookies.Delete(name)
            );

            var result = await tokenService.RefreshTokenAsync(refreshToken, authContext, cancellationToken);

            if (result?.Success == true)
            {
                return Results.Ok(new TokenResponse(
                    AccessToken: result.AccessToken,
                    ExpiresIn: result.ExpiresIn,
                    ExpiresAt: result.ExpiresAt));
            }

            return Results.Unauthorized();
        }
        catch (Exception ex)
        {
            return Results.Problem($"Token refresh failed: {ex.Message}");
        }
    }

    private static async Task<IResult> HandleLogout(
        HttpContext context,
        ITokenService tokenService,
        CancellationToken cancellationToken)
    {
        try
        {
            var refreshToken = context.Request.Cookies["refresh_token"];
            if (!string.IsNullOrEmpty(refreshToken))
            {
                await tokenService.RevokeRefreshTokenAsync(refreshToken, "User logout", cancellationToken);
            }

            // Clear refresh token cookie
            context.Response.Cookies.Delete("refresh_token");

            return Results.Ok(new { message = "Logged out successfully" });
        }
        catch (Exception ex)
        {
            return Results.Problem($"Logout failed: {ex.Message}");
        }
    }

    private static async Task<IResult> HandleRegister(
        HttpContext context,
        IAuthService authService,
        CancellationToken cancellationToken)
    {
        try
        {
            var request = await context.Request.ReadFromJsonAsync<RegisterRequest>(cancellationToken);
            if (request == null)
            {
                return Results.BadRequest(new { message = "Invalid request body" });
            }

            var registerRequest = new Application.Services.RegisterRequest(
                request.Email, 
                request.Password, 
                request.FirstName, 
                request.LastName);

            var result = await authService.RegisterAsync(registerRequest, cancellationToken);

            if (result.Success)
            {
                return Results.Ok(new { message = result.Message, userId = result.UserId });
            }

            return Results.BadRequest(new { message = result.Message });
        }
        catch (Exception ex)
        {
            return Results.Problem($"Registration failed: {ex.Message}");
        }
    }

    private static string GetClientIpAddress(HttpContext context)
    {
        return context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    }
}

// Request/Response models
public record LoginRequest(string Email, string Password);
public record RegisterRequest(string Email, string Password, string FirstName, string LastName);
public record TokenResponse(string AccessToken, int ExpiresIn, DateTime ExpiresAt);
