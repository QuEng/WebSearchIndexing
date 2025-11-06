namespace WebSearchIndexing.Modules.Identity.Application.Security;

/// <summary>
/// Password policy configuration options
/// </summary>
public sealed class PasswordPolicyOptions
{
    /// <summary>
    /// Minimum password length
    /// </summary>
    public int MinimumLength { get; set; } = 8;

    /// <summary>
    /// Maximum password length
    /// </summary>
    public int MaximumLength { get; set; } = 128;

    /// <summary>
    /// Require at least one uppercase letter
    /// </summary>
    public bool RequireUppercase { get; set; } = true;

    /// <summary>
    /// Require at least one lowercase letter
    /// </summary>
    public bool RequireLowercase { get; set; } = true;

    /// <summary>
    /// Require at least one digit
    /// </summary>
    public bool RequireDigit { get; set; } = true;

    /// <summary>
    /// Require at least one special character
    /// </summary>
    public bool RequireSpecialCharacter { get; set; } = true;

    /// <summary>
    /// Prevent password reuse (number of previous passwords to check)
    /// </summary>
    public int PasswordHistoryLimit { get; set; } = 5;

    /// <summary>
    /// Minimum password age in days (0 = no minimum)
    /// </summary>
    public int MinimumPasswordAgeDays { get; set; } = 0;

    /// <summary>
    /// Maximum password age in days (0 = no expiration)
    /// </summary>
    public int MaximumPasswordAgeDays { get; set; } = 90;

    /// <summary>
    /// List of common passwords to reject
    /// </summary>
    public List<string> CommonPasswordBlacklist { get; set; } = new()
    {
        "password", "password123", "12345678", "qwerty", "admin", "letmein"
    };
}

/// <summary>
/// Result of password validation
/// </summary>
public sealed class PasswordValidationResult
{
    public bool IsValid { get; init; }
    public List<string> Errors { get; init; } = new();

    public static PasswordValidationResult Success() => new() { IsValid = true };
    
    public static PasswordValidationResult Failure(params string[] errors) => new()
    {
        IsValid = false,
        Errors = errors.ToList()
    };
}

/// <summary>
/// Service for enforcing password policies
/// </summary>
public interface IPasswordPolicyService
{
    /// <summary>
    /// Validates password against policy
    /// </summary>
    PasswordValidationResult ValidatePassword(string password, string? email = null);

    /// <summary>
    /// Checks if password can be changed (respects minimum age)
    /// </summary>
    Task<bool> CanChangePasswordAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if password is expired
    /// </summary>
    Task<bool> IsPasswordExpiredAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Records password change for history tracking
    /// </summary>
    Task RecordPasswordChangeAsync(Guid userId, string passwordHash, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if password was used recently (in history)
    /// </summary>
    Task<bool> WasPasswordUsedRecentlyAsync(Guid userId, string passwordHash, CancellationToken cancellationToken = default);
}
