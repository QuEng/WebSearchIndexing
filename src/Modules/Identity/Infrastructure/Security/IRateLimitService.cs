using System.Collections.Concurrent;

namespace WebSearchIndexing.Modules.Identity.Infrastructure.Security;

/// <summary>
/// Service for tracking and enforcing rate limits on authentication endpoints
/// </summary>
public interface IRateLimitService
{
    /// <summary>
    /// Checks if the request is allowed based on rate limiting rules
    /// </summary>
    Task<bool> IsRequestAllowedAsync(string key, string endpoint, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the remaining attempts for a specific key and endpoint
    /// </summary>
    Task<int> GetRemainingAttemptsAsync(string key, string endpoint, CancellationToken cancellationToken = default);

    /// <summary>
    /// Resets the rate limit counter for a specific key and endpoint
    /// </summary>
    Task ResetLimitAsync(string key, string endpoint, CancellationToken cancellationToken = default);
}

/// <summary>
/// In-memory implementation of rate limiting service
/// </summary>
public sealed class InMemoryRateLimitService : IRateLimitService
{
    private readonly ConcurrentDictionary<string, RateLimitEntry> _entries = new();
    private readonly RateLimitOptions _options;

    public InMemoryRateLimitService(RateLimitOptions options)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
    }

    public Task<bool> IsRequestAllowedAsync(string key, string endpoint, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(key);
        ArgumentNullException.ThrowIfNull(endpoint);

        var limits = GetLimitsForEndpoint(endpoint);
        var entryKey = $"{key}:{endpoint}";
        
        var entry = _entries.GetOrAdd(entryKey, _ => new RateLimitEntry
        {
            Count = 0,
            WindowStart = DateTime.UtcNow
        });

        lock (entry)
        {
            // Check if window has expired
            if (DateTime.UtcNow - entry.WindowStart > TimeSpan.FromMinutes(limits.WindowMinutes))
            {
                // Reset counter
                entry.Count = 0;
                entry.WindowStart = DateTime.UtcNow;
            }

            // Check if limit exceeded
            if (entry.Count >= limits.Limit)
            {
                return Task.FromResult(false);
            }

            // Increment counter
            entry.Count++;
            return Task.FromResult(true);
        }
    }

    public Task<int> GetRemainingAttemptsAsync(string key, string endpoint, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(key);
        ArgumentNullException.ThrowIfNull(endpoint);

        var limits = GetLimitsForEndpoint(endpoint);
        var entryKey = $"{key}:{endpoint}";

        if (!_entries.TryGetValue(entryKey, out var entry))
        {
            return Task.FromResult(limits.Limit);
        }

        lock (entry)
        {
            // Check if window has expired
            if (DateTime.UtcNow - entry.WindowStart > TimeSpan.FromMinutes(limits.WindowMinutes))
            {
                return Task.FromResult(limits.Limit);
            }

            return Task.FromResult(Math.Max(0, limits.Limit - entry.Count));
        }
    }

    public Task ResetLimitAsync(string key, string endpoint, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(key);
        ArgumentNullException.ThrowIfNull(endpoint);

        var entryKey = $"{key}:{endpoint}";
        _entries.TryRemove(entryKey, out _);

        return Task.CompletedTask;
    }

    private (int Limit, int WindowMinutes) GetLimitsForEndpoint(string endpoint)
    {
        return endpoint.ToLowerInvariant() switch
        {
            "login" => (_options.LoginAttemptsLimit, _options.LoginAttemptsWindowMinutes),
            "register" => (_options.RegistrationAttemptsLimit, _options.RegistrationAttemptsWindowMinutes),
            "refresh" => (_options.RefreshAttemptsLimit, _options.RefreshAttemptsWindowMinutes),
            "passwordreset" => (_options.PasswordResetAttemptsLimit, _options.PasswordResetAttemptsWindowMinutes),
            _ => (100, 60) // Default fallback
        };
    }

    private sealed class RateLimitEntry
    {
        public int Count { get; set; }
        public DateTime WindowStart { get; set; }
    }
}
