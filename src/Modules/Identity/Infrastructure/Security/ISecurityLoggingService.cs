using Microsoft.Extensions.Logging;
using WebSearchIndexing.Modules.Identity.Application.Security;

namespace WebSearchIndexing.Modules.Identity.Infrastructure.Security;

/// <summary>
/// Implementation of security logging service
/// </summary>
public sealed class SecurityLoggingService : ISecurityLoggingService
{
    private readonly ILogger<SecurityLoggingService> _logger;

    public SecurityLoggingService(ILogger<SecurityLoggingService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public void LogSecurityEvent(SecurityEvent securityEvent)
    {
        ArgumentNullException.ThrowIfNull(securityEvent);

        var logLevel = GetLogLevel(securityEvent.EventType);
        var message = FormatSecurityMessage(securityEvent);

        _logger.Log(logLevel, "[SECURITY] {EventType} - {Message}", 
            securityEvent.EventType, message);
    }

    public Task LogSecurityEventAsync(SecurityEvent securityEvent, CancellationToken cancellationToken = default)
    {
        LogSecurityEvent(securityEvent);
        return Task.CompletedTask;
    }

    private static LogLevel GetLogLevel(SecurityEventType eventType)
    {
        return eventType switch
        {
            SecurityEventType.LoginSuccess => LogLevel.Information,
            SecurityEventType.LoginFailure => LogLevel.Warning,
            SecurityEventType.LoginRateLimitExceeded => LogLevel.Warning,
            SecurityEventType.RegistrationSuccess => LogLevel.Information,
            SecurityEventType.RegistrationFailure => LogLevel.Warning,
            SecurityEventType.RegistrationRateLimitExceeded => LogLevel.Warning,
            SecurityEventType.TokenRefreshSuccess => LogLevel.Debug,
            SecurityEventType.TokenRefreshFailure => LogLevel.Warning,
            SecurityEventType.TokenRefreshRateLimitExceeded => LogLevel.Warning,
            SecurityEventType.LogoutSuccess => LogLevel.Information,
            SecurityEventType.PasswordChangeSuccess => LogLevel.Information,
            SecurityEventType.PasswordChangeFailure => LogLevel.Warning,
            SecurityEventType.AccountLockout => LogLevel.Error,
            SecurityEventType.AccountUnlock => LogLevel.Information,
            SecurityEventType.InvalidTokenAttempt => LogLevel.Warning,
            SecurityEventType.CookieSecurityViolation => LogLevel.Error,
            SecurityEventType.SuspiciousActivity => LogLevel.Error,
            SecurityEventType.EmailVerificationSent => LogLevel.Information,
            SecurityEventType.EmailVerified => LogLevel.Information,
            _ => LogLevel.Information
        };
    }

    private static string FormatSecurityMessage(SecurityEvent securityEvent)
    {
        var parts = new List<string>();

        if (!string.IsNullOrEmpty(securityEvent.Email))
            parts.Add($"Email: {securityEvent.Email}");

        if (!string.IsNullOrEmpty(securityEvent.UserId))
            parts.Add($"UserId: {securityEvent.UserId}");

        if (!string.IsNullOrEmpty(securityEvent.IpAddress))
            parts.Add($"IP: {securityEvent.IpAddress}");

        if (!string.IsNullOrEmpty(securityEvent.CorrelationId))
            parts.Add($"CorrelationId: {securityEvent.CorrelationId}");

        if (!string.IsNullOrEmpty(securityEvent.AdditionalInfo))
            parts.Add(securityEvent.AdditionalInfo);

        return string.Join(", ", parts);
    }
}
