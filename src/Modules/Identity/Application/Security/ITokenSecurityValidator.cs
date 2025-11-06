using System.Security.Claims;

namespace WebSearchIndexing.Modules.Identity.Application.Security;

/// <summary>
/// Security validation result for tokens
/// </summary>
public sealed class TokenSecurityValidationResult
{
    public bool IsValid { get; init; }
    public string? ErrorMessage { get; init; }
    public List<string> SecurityWarnings { get; init; } = new();

    public static TokenSecurityValidationResult Success() => new() { IsValid = true };
    
    public static TokenSecurityValidationResult Failure(string errorMessage, params string[] warnings) => new()
    {
        IsValid = false,
        ErrorMessage = errorMessage,
        SecurityWarnings = warnings.ToList()
    };

    public static TokenSecurityValidationResult Warning(params string[] warnings) => new()
    {
        IsValid = true,
        SecurityWarnings = warnings.ToList()
    };
}

/// <summary>
/// Service for validating token security
/// </summary>
public interface ITokenSecurityValidator
{
    /// <summary>
    /// Validates token claims for security issues
    /// </summary>
    TokenSecurityValidationResult ValidateTokenClaims(ClaimsPrincipal principal);

    /// <summary>
    /// Checks if token is from a suspicious source
    /// </summary>
    Task<bool> IsTokenFromSuspiciousSourceAsync(string token, string? ipAddress, string? userAgent, CancellationToken cancellationToken = default);

    /// <summary>
    /// Records token usage for anomaly detection
    /// </summary>
    Task RecordTokenUsageAsync(Guid userId, string token, string? ipAddress, string? userAgent, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks for token replay attacks
    /// </summary>
    Task<bool> IsTokenReplayedAsync(string jti, CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks token as used (for replay attack prevention)
    /// </summary>
    Task MarkTokenAsUsedAsync(string jti, DateTime expiresAt, CancellationToken cancellationToken = default);
}
