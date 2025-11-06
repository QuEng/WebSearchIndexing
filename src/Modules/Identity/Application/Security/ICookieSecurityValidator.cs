namespace WebSearchIndexing.Modules.Identity.Application.Security;

/// <summary>
/// Cookie security configuration options
/// </summary>
public sealed class CookieSecurityOptions
{
    /// <summary>
    /// Require cookies to be sent over HTTPS only
    /// </summary>
    public bool RequireHttps { get; set; } = true;

    /// <summary>
    /// HttpOnly flag prevents JavaScript access to cookies
    /// </summary>
    public bool HttpOnly { get; set; } = true;

    /// <summary>
    /// SameSite policy for CSRF protection
    /// </summary>
    public string SameSite { get; set; } = "Strict";

    /// <summary>
    /// Cookie expiration in minutes
    /// </summary>
    public int ExpirationMinutes { get; set; } = 60;

    /// <summary>
    /// Sliding expiration - renew cookie on each request
    /// </summary>
    public bool SlidingExpiration { get; set; } = true;

    /// <summary>
    /// Cookie domain
    /// </summary>
    public string? Domain { get; set; }

    /// <summary>
    /// Cookie path
    /// </summary>
    public string Path { get; set; } = "/";
}

/// <summary>
/// Cookie security validation result
/// </summary>
public sealed class CookieSecurityValidationResult
{
    public bool IsSecure { get; init; }
    public List<string> Violations { get; init; } = new();

    public static CookieSecurityValidationResult Secure() => new() { IsSecure = true };
    
    public static CookieSecurityValidationResult Insecure(params string[] violations) => new()
    {
        IsSecure = false,
        Violations = violations.ToList()
    };
}

/// <summary>
/// Service for validating cookie security
/// </summary>
public interface ICookieSecurityValidator
{
    /// <summary>
    /// Validates cookie security settings
    /// </summary>
    CookieSecurityValidationResult ValidateCookieSettings(CookieSecurityOptions options);

    /// <summary>
    /// Checks if request meets cookie security requirements
    /// </summary>
    bool IsRequestSecure(bool isHttps, string? origin, string? referer);

    /// <summary>
    /// Validates SameSite policy
    /// </summary>
    bool ValidateSameSitePolicy(string? origin, string? referer, string expectedDomain);
}
