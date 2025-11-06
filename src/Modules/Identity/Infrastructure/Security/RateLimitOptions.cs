namespace WebSearchIndexing.Modules.Identity.Infrastructure.Security;

/// <summary>
/// Configuration options for rate limiting authentication endpoints
/// </summary>
public sealed class RateLimitOptions
{
    /// <summary>
    /// Maximum number of login attempts per time window
    /// </summary>
    public int LoginAttemptsLimit { get; set; } = 5;

    /// <summary>
    /// Time window in minutes for login attempts
    /// </summary>
    public int LoginAttemptsWindowMinutes { get; set; } = 15;

    /// <summary>
    /// Maximum number of registration attempts per time window
    /// </summary>
    public int RegistrationAttemptsLimit { get; set; } = 3;

    /// <summary>
    /// Time window in minutes for registration attempts
    /// </summary>
    public int RegistrationAttemptsWindowMinutes { get; set; } = 60;

    /// <summary>
    /// Maximum number of token refresh attempts per time window
    /// </summary>
    public int RefreshAttemptsLimit { get; set; } = 10;

    /// <summary>
    /// Time window in minutes for token refresh attempts
    /// </summary>
    public int RefreshAttemptsWindowMinutes { get; set; } = 60;

    /// <summary>
    /// Maximum number of password reset requests per time window
    /// </summary>
    public int PasswordResetAttemptsLimit { get; set; } = 3;

    /// <summary>
    /// Time window in minutes for password reset requests
    /// </summary>
    public int PasswordResetAttemptsWindowMinutes { get; set; } = 60;
}
