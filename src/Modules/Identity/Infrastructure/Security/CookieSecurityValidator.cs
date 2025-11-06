using Microsoft.Extensions.Logging;
using WebSearchIndexing.Modules.Identity.Application.Security;

namespace WebSearchIndexing.Modules.Identity.Infrastructure.Security;

/// <summary>
/// Implementation of cookie security validator
/// </summary>
public sealed class CookieSecurityValidator : ICookieSecurityValidator
{
    private readonly ILogger<CookieSecurityValidator> _logger;

    public CookieSecurityValidator(ILogger<CookieSecurityValidator> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public CookieSecurityValidationResult ValidateCookieSettings(CookieSecurityOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        var violations = new List<string>();

        // Check HTTPS requirement
        if (!options.RequireHttps)
        {
            violations.Add("Cookies should require HTTPS for security");
        }

        // Check HttpOnly flag
        if (!options.HttpOnly)
        {
            violations.Add("Cookies should have HttpOnly flag to prevent XSS attacks");
        }

        // Check SameSite policy
        var validSameSitePolicies = new[] { "Strict", "Lax", "None" };
        if (!validSameSitePolicies.Contains(options.SameSite, StringComparer.OrdinalIgnoreCase))
        {
            violations.Add($"Invalid SameSite policy: {options.SameSite}. Valid values: Strict, Lax, None");
        }

        if (options.SameSite.Equals("None", StringComparison.OrdinalIgnoreCase) && !options.RequireHttps)
        {
            violations.Add("SameSite=None requires Secure flag (HTTPS)");
        }

        // Check expiration
        if (options.ExpirationMinutes <= 0)
        {
            violations.Add("Cookie expiration must be positive");
        }

        if (options.ExpirationMinutes > 1440) // 24 hours
        {
            violations.Add($"Cookie expiration is very long: {options.ExpirationMinutes} minutes. Consider shorter duration for security");
        }

        // Log violations
        if (violations.Any())
        {
            _logger.LogWarning("Cookie security violations detected: {Violations}", string.Join(", ", violations));
        }

        return violations.Any()
            ? CookieSecurityValidationResult.Insecure(violations.ToArray())
            : CookieSecurityValidationResult.Secure();
    }

    public bool IsRequestSecure(bool isHttps, string? origin, string? referer)
    {
        // Check HTTPS
        if (!isHttps)
        {
            _logger.LogWarning("Insecure HTTP request detected. Origin: {Origin}, Referer: {Referer}", origin, referer);
            return false;
        }

        // Check for mixed content
        if (!string.IsNullOrWhiteSpace(origin) && origin.StartsWith("http://", StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogWarning("HTTP Origin header in HTTPS request: {Origin}", origin);
            return false;
        }

        if (!string.IsNullOrWhiteSpace(referer) && referer.StartsWith("http://", StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogWarning("HTTP Referer header in HTTPS request: {Referer}", referer);
            return false;
        }

        return true;
    }

    public bool ValidateSameSitePolicy(string? origin, string? referer, string expectedDomain)
    {
        if (string.IsNullOrWhiteSpace(expectedDomain))
        {
            return true; // Cannot validate without expected domain
        }

        // Check Origin header
        if (!string.IsNullOrWhiteSpace(origin))
        {
            if (!IsFromSameSite(origin, expectedDomain))
            {
                _logger.LogWarning("Cross-site request detected. Origin: {Origin}, Expected: {Expected}", origin, expectedDomain);
                return false;
            }
        }

        // Check Referer header as fallback
        if (!string.IsNullOrWhiteSpace(referer))
        {
            if (!IsFromSameSite(referer, expectedDomain))
            {
                _logger.LogWarning("Cross-site request detected. Referer: {Referer}, Expected: {Expected}", referer, expectedDomain);
                return false;
            }
        }

        return true;
    }

    private static bool IsFromSameSite(string url, string expectedDomain)
    {
        try
        {
            var uri = new Uri(url);
            return uri.Host.Equals(expectedDomain, StringComparison.OrdinalIgnoreCase) ||
                   uri.Host.EndsWith($".{expectedDomain}", StringComparison.OrdinalIgnoreCase);
        }
        catch
        {
            return false;
        }
    }
}
