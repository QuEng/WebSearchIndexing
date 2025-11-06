namespace WebSearchIndexing.Modules.Identity.Api.Models;

/// <summary>
/// Request model for user login
/// </summary>
public sealed record LoginRequest
{
    /// <summary>
    /// User's email address
    /// </summary>
    public string Email { get; init; } = string.Empty;

    /// <summary>
    /// User's password
    /// </summary>
    public string Password { get; init; } = string.Empty;
}

/// <summary>
/// Request model for user registration
/// </summary>
public sealed record RegisterRequest
{
    /// <summary>
    /// User's email address
    /// </summary>
    public string Email { get; init; } = string.Empty;

    /// <summary>
    /// User's password
    /// </summary>
    public string Password { get; init; } = string.Empty;

    /// <summary>
    /// Password confirmation
    /// </summary>
    public string ConfirmPassword { get; init; } = string.Empty;

    /// <summary>
    /// User's first name
    /// </summary>
    public string FirstName { get; init; } = string.Empty;

    /// <summary>
    /// User's last name
    /// </summary>
    public string LastName { get; init; } = string.Empty;
}

/// <summary>
/// Request model for changing password
/// </summary>
public sealed record ChangePasswordRequest
{
    /// <summary>
    /// Current password
    /// </summary>
    public string CurrentPassword { get; init; } = string.Empty;

    /// <summary>
    /// New password
    /// </summary>
    public string NewPassword { get; init; } = string.Empty;

    /// <summary>
    /// New password confirmation
    /// </summary>
    public string ConfirmPassword { get; init; } = string.Empty;
}
