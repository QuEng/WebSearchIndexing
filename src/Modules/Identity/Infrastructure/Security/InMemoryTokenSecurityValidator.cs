using System.Collections.Concurrent;
using System.Security.Claims;
using Microsoft.Extensions.Logging;
using WebSearchIndexing.Modules.Identity.Application.Security;

namespace WebSearchIndexing.Modules.Identity.Infrastructure.Security;

/// <summary>
/// In-memory implementation of token security validator
/// </summary>
public sealed class InMemoryTokenSecurityValidator : ITokenSecurityValidator
{
    private readonly ILogger<InMemoryTokenSecurityValidator> _logger;
    private readonly ConcurrentDictionary<string, DateTime> _usedTokens = new();
    private readonly ConcurrentDictionary<Guid, List<TokenUsageRecord>> _tokenUsageHistory = new();

    public InMemoryTokenSecurityValidator(ILogger<InMemoryTokenSecurityValidator> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public TokenSecurityValidationResult ValidateTokenClaims(ClaimsPrincipal principal)
    {
        ArgumentNullException.ThrowIfNull(principal);

        var warnings = new List<string>();

        // Check for required claims
        var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return TokenSecurityValidationResult.Failure("Token is missing user identifier claim");
        }

        var email = principal.FindFirst(ClaimTypes.Email)?.Value;
        if (string.IsNullOrEmpty(email))
        {
            warnings.Add("Token is missing email claim");
        }

        var jti = principal.FindFirst("jti")?.Value;
        if (string.IsNullOrEmpty(jti))
        {
            warnings.Add("Token is missing JTI (JWT ID) claim - replay attack prevention disabled");
        }

        // Check token expiration claim exists
        var exp = principal.FindFirst("exp")?.Value;
        if (string.IsNullOrEmpty(exp))
        {
            return TokenSecurityValidationResult.Failure("Token is missing expiration claim");
        }

        // Validate expiration time
        if (long.TryParse(exp, out var expTimestamp))
        {
            var expirationTime = DateTimeOffset.FromUnixTimeSeconds(expTimestamp).UtcDateTime;
            if (expirationTime < DateTime.UtcNow)
            {
                return TokenSecurityValidationResult.Failure("Token has expired");
            }

            // Warn if token has very long expiration
            var timeUntilExpiration = expirationTime - DateTime.UtcNow;
            if (timeUntilExpiration.TotalDays > 30)
            {
                warnings.Add($"Token has unusually long expiration: {timeUntilExpiration.TotalDays:F0} days");
            }
        }

        return warnings.Any() 
            ? TokenSecurityValidationResult.Warning(warnings.ToArray())
            : TokenSecurityValidationResult.Success();
    }

    public Task<bool> IsTokenFromSuspiciousSourceAsync(
        string token, 
        string? ipAddress, 
        string? userAgent, 
        CancellationToken cancellationToken = default)
    {
        // Simple heuristic checks
        var isSuspicious = false;

        // Check for missing user agent (potential bot)
        if (string.IsNullOrWhiteSpace(userAgent))
        {
            _logger.LogWarning("Token request from IP {IpAddress} with no User-Agent", ipAddress);
            isSuspicious = true;
        }

        // Check for suspicious user agents
        if (!string.IsNullOrWhiteSpace(userAgent))
        {
            var suspiciousPatterns = new[] { "bot", "crawler", "spider", "scraper" };
            if (suspiciousPatterns.Any(pattern => 
                userAgent.Contains(pattern, StringComparison.OrdinalIgnoreCase)))
            {
                _logger.LogWarning("Token request from suspicious User-Agent: {UserAgent}", userAgent);
                isSuspicious = true;
            }
        }

        return Task.FromResult(isSuspicious);
    }

    public Task RecordTokenUsageAsync(
        Guid userId, 
        string token, 
        string? ipAddress, 
        string? userAgent, 
        CancellationToken cancellationToken = default)
    {
        var record = new TokenUsageRecord
        {
            Token = token,
            IpAddress = ipAddress,
            UserAgent = userAgent,
            UsedAt = DateTime.UtcNow
        };

        _tokenUsageHistory.AddOrUpdate(
            userId,
            new List<TokenUsageRecord> { record },
            (_, existing) =>
            {
                existing.Add(record);
                // Keep only last 100 records per user
                if (existing.Count > 100)
                {
                    existing = existing.OrderByDescending(r => r.UsedAt).Take(100).ToList();
                }
                return existing;
            });

        return Task.CompletedTask;
    }

    public Task<bool> IsTokenReplayedAsync(string jti, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(jti))
        {
            return Task.FromResult(false);
        }

        // Clean up expired tokens first
        CleanupExpiredTokens();

        return Task.FromResult(_usedTokens.ContainsKey(jti));
    }

    public Task MarkTokenAsUsedAsync(string jti, DateTime expiresAt, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(jti))
        {
            return Task.CompletedTask;
        }

        _usedTokens.TryAdd(jti, expiresAt);
        return Task.CompletedTask;
    }

    private void CleanupExpiredTokens()
    {
        var now = DateTime.UtcNow;
        var expiredTokens = _usedTokens
            .Where(kvp => kvp.Value < now)
            .Select(kvp => kvp.Key)
            .ToList();

        foreach (var jti in expiredTokens)
        {
            _usedTokens.TryRemove(jti, out _);
        }
    }

    private sealed class TokenUsageRecord
    {
        public string Token { get; init; } = default!;
        public string? IpAddress { get; init; }
        public string? UserAgent { get; init; }
        public DateTime UsedAt { get; init; }
    }
}
