using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using WebSearchIndexing.Modules.Identity.Application.Abstractions;
using WebSearchIndexing.Modules.Identity.Domain.Repositories;

namespace WebSearchIndexing.Modules.Identity.Api.Endpoints;

internal static class SecurityEndpoints
{
    public static RouteGroupBuilder MapSecurityEndpoints(this RouteGroupBuilder identityGroup)
    {
        ArgumentNullException.ThrowIfNull(identityGroup);

        var securityGroup = identityGroup.MapGroup("/security").RequireAuthorization();

        securityGroup.MapGet("/login-history", HandleGetLoginHistory);
        securityGroup.MapGet("/active-sessions", HandleGetActiveSessions);
        securityGroup.MapDelete("/sessions/{sessionId:guid}", HandleRevokeSession);

        return identityGroup;
    }

    private static async Task<IResult> HandleGetLoginHistory(
        HttpContext context,
        ILoginHistoryRepository loginHistoryRepository,
        CancellationToken cancellationToken)
    {
        try
        {
            var userId = context.User.FindFirst("user_id")?.Value;
            if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userGuid))
            {
                return Results.Unauthorized();
            }

            // Get recent login history (last 30 days, limit 50)
            var loginHistory = await loginHistoryRepository.GetRecentByUserIdAsync(
                userGuid, 
                days: 30,
                limit: 50, 
                cancellationToken);

            var response = loginHistory.Select(lh => new LoginHistoryResponse(
                lh.Id,
                lh.IpAddress,
                lh.UserAgent,
                lh.Location,
                lh.LoginAt,
                lh.IsSuccessful,
                lh.FailureReason,
                lh.DeviceInfo
            )).ToList();

            return Results.Ok(response);
        }
        catch (Exception ex)
        {
            return Results.Problem($"Failed to get login history: {ex.Message}");
        }
    }

    private static async Task<IResult> HandleGetActiveSessions(
        HttpContext context,
        IRefreshTokenRepository refreshTokenRepository,
        CancellationToken cancellationToken)
    {
        try
        {
            var userId = context.User.FindFirst("user_id")?.Value;
            if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userGuid))
            {
                return Results.Unauthorized();
            }

            // Get all valid (non-revoked, non-expired) refresh tokens for the user
            var activeSessions = await refreshTokenRepository.GetActiveByUserIdAsync(userGuid, cancellationToken);

            var response = activeSessions.Select(session => new ActiveSessionResponse(
                session.Id,
                session.CreatedByIp,
                session.CreatedAt,
                session.ExpiresAt,
                DetermineDeviceType(session.CreatedByIp), // Simple device detection
                GetLocationFromIp(session.CreatedByIp), // Simple location detection
                IsCurrentSession(context, session.Token)
            )).ToList();

            return Results.Ok(response);
        }
        catch (Exception ex)
        {
            return Results.Problem($"Failed to get active sessions: {ex.Message}");
        }
    }

    private static async Task<IResult> HandleRevokeSession(
        HttpContext context,
        Guid sessionId,
        IRefreshTokenRepository refreshTokenRepository,
        CancellationToken cancellationToken)
    {
        try
        {
            var userId = context.User.FindFirst("user_id")?.Value;
            if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userGuid))
            {
                return Results.Unauthorized();
            }

            // Get the session
            var session = await refreshTokenRepository.GetByIdAsync(sessionId, cancellationToken);
            if (session == null)
            {
                return Results.NotFound(new { message = "Session not found" });
            }

            // Verify the session belongs to the current user
            if (session.UserId != userGuid)
            {
                return Results.Forbid();
            }

            // Don't allow revoking the current session
            if (IsCurrentSession(context, session.Token))
            {
                return Results.BadRequest(new { message = "Cannot revoke the current session" });
            }

            // Revoke the session
            session.Revoke("Revoked by user");
            await refreshTokenRepository.UpdateAsync(session, cancellationToken);

            return Results.Ok(new { message = "Session revoked successfully" });
        }
        catch (Exception ex)
        {
            return Results.Problem($"Failed to revoke session: {ex.Message}");
        }
    }

    private static string DetermineDeviceType(string ipAddress)
    {
        // Simple device type detection - can be enhanced with user agent analysis
        return "Unknown Device";
    }

    private static string GetLocationFromIp(string ipAddress)
    {
        // Simple location detection - in production, use a geolocation service
        return ipAddress.StartsWith("192.168.") || 
               ipAddress.StartsWith("10.") || 
               ipAddress.StartsWith("172.") || 
               ipAddress == "127.0.0.1" || 
               ipAddress == "::1"
            ? "Local Network"
            : "Unknown Location";
    }

    private static bool IsCurrentSession(HttpContext context, string sessionToken)
    {
        // Check if this is the current session by comparing tokens
        // In production, you might want to store the current session ID in the JWT or context
        var currentToken = context.Request.Cookies["refresh_token"];
        return !string.IsNullOrEmpty(currentToken) && currentToken == sessionToken;
    }
}

// Response models
public record LoginHistoryResponse(
    Guid Id,
    string IpAddress,
    string UserAgent,
    string? Location,
    DateTime LoginAt,
    bool IsSuccessful,
    string? FailureReason,
    string? DeviceInfo);

public record ActiveSessionResponse(
    Guid Id,
    string IpAddress,
    DateTime CreatedAt,
    DateTime ExpiresAt,
    string DeviceType,
    string Location,
    bool IsCurrent);
