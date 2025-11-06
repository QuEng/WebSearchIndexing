namespace WebSearchIndexing.Modules.Identity.Application.Security;

/// <summary>
/// Types of security events to log
/// </summary>
public enum SecurityEventType
{
    LoginSuccess,
    LoginFailure,
    LoginRateLimitExceeded,
    RegistrationSuccess,
    RegistrationFailure,
    RegistrationRateLimitExceeded,
    TokenRefreshSuccess,
    TokenRefreshFailure,
    TokenRefreshRateLimitExceeded,
    LogoutSuccess,
    PasswordChangeSuccess,
    PasswordChangeFailure,
    AccountLockout,
    AccountUnlock,
    InvalidTokenAttempt,
    CookieSecurityViolation,
    SuspiciousActivity,
    EmailVerificationSent,
    EmailVerified
}

/// <summary>
/// Security event data for logging
/// </summary>
public sealed class SecurityEvent
{
    public SecurityEventType EventType { get; init; }
    public string? UserId { get; init; }
    public string? Email { get; init; }
    public string? IpAddress { get; init; }
    public string? UserAgent { get; init; }
    public string? AdditionalInfo { get; init; }
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
    public string? CorrelationId { get; init; }
}

/// <summary>
/// Service for logging security-related events
/// </summary>
public interface ISecurityLoggingService
{
    /// <summary>
    /// Logs a security event
    /// </summary>
    void LogSecurityEvent(SecurityEvent securityEvent);

    /// <summary>
    /// Logs a security event asynchronously
    /// </summary>
    Task LogSecurityEventAsync(SecurityEvent securityEvent, CancellationToken cancellationToken = default);
}
