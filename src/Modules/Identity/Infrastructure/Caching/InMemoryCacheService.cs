using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using WebSearchIndexing.Modules.Identity.Application.Caching;
using WebSearchIndexing.Modules.Identity.Application.Security;

namespace WebSearchIndexing.Modules.Identity.Infrastructure.Caching;

/// <summary>
/// In-memory implementation of cache service with security awareness
/// </summary>
public sealed class InMemoryCacheService : ICacheService
{
    private readonly ConcurrentDictionary<string, CacheEntry> _cache = new();
    private readonly ILogger<InMemoryCacheService> _logger;
    private readonly ISecurityLoggingService _securityLogging;
    private int _hitCount;
    private int _missCount;
    private readonly ConcurrentDictionary<CacheInvalidationReason, int> _invalidationCounts = new();

    public InMemoryCacheService(
        ILogger<InMemoryCacheService> logger,
        ISecurityLoggingService securityLogging)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _securityLogging = securityLogging ?? throw new ArgumentNullException(nameof(securityLogging));

        // Start cleanup task
        _ = Task.Run(CleanupExpiredEntriesAsync);
    }

    public async Task<T?> GetOrCreateAsync<T>(
        string key,
        Func<Task<T>> factory,
        TimeSpan? expiration = null,
        CancellationToken cancellationToken = default) where T : class
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        ArgumentNullException.ThrowIfNull(factory);

        // Try get from cache
        var cached = await GetAsync<T>(key, cancellationToken);
        if (cached != null)
        {
            return cached;
        }

        // Create new value
        var value = await factory();
        if (value != null)
        {
            await SetAsync(key, value, expiration, cancellationToken);
        }

        return value;
    }

    public Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) where T : class
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        if (_cache.TryGetValue(key, out var entry))
        {
            if (entry.IsExpired)
            {
                // Remove expired entry
                _cache.TryRemove(key, out _);
                Interlocked.Increment(ref _missCount);
                return Task.FromResult<T?>(null);
            }

            Interlocked.Increment(ref _hitCount);
            return Task.FromResult(entry.Value as T);
        }

        Interlocked.Increment(ref _missCount);
        return Task.FromResult<T?>(null);
    }

    public Task SetAsync<T>(
        string key,
        T value,
        TimeSpan? expiration = null,
        CancellationToken cancellationToken = default) where T : class
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        ArgumentNullException.ThrowIfNull(value);

        var entry = new CacheEntry
        {
            Value = value,
            ExpiresAt = expiration.HasValue 
                ? DateTime.UtcNow.Add(expiration.Value) 
                : DateTime.UtcNow.AddHours(1) // Default 1 hour
        };

        _cache[key] = entry;
        return Task.CompletedTask;
    }

    public Task RemoveAsync(
        string key,
        CacheInvalidationReason reason = CacheInvalidationReason.ManualInvalidation,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        if (_cache.TryRemove(key, out _))
        {
            IncrementInvalidationCount(reason);
            _logger.LogDebug("Cache key {Key} invalidated. Reason: {Reason}", key, reason);
        }

        return Task.CompletedTask;
    }

    public Task RemoveByPatternAsync(
        string pattern,
        CacheInvalidationReason reason = CacheInvalidationReason.ManualInvalidation,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(pattern);

        var keysToRemove = _cache.Keys.Where(k => MatchesPattern(k, pattern)).ToList();
        
        foreach (var key in keysToRemove)
        {
            if (_cache.TryRemove(key, out _))
            {
                IncrementInvalidationCount(reason);
            }
        }

        if (keysToRemove.Any())
        {
            _logger.LogDebug(
                "Removed {Count} cache entries matching pattern {Pattern}. Reason: {Reason}",
                keysToRemove.Count, pattern, reason);
        }

        return Task.CompletedTask;
    }

    public Task InvalidateUserCacheAsync(
        Guid userId,
        CacheInvalidationReason reason = CacheInvalidationReason.TokenRefresh,
        CancellationToken cancellationToken = default)
    {
        // Invalidate all user-related cache entries
        var pattern = $"user:{userId}:*";
        
        _securityLogging.LogSecurityEvent(new SecurityEvent
        {
            EventType = SecurityEventType.SuspiciousActivity, // Reusing for cache invalidation
            UserId = userId.ToString(),
            AdditionalInfo = $"User cache invalidated. Reason: {reason}"
        });

        return RemoveByPatternAsync(pattern, reason, cancellationToken);
    }

    public Task InvalidateTenantCacheAsync(
        Guid tenantId,
        CacheInvalidationReason reason = CacheInvalidationReason.SecurityPolicyUpdate,
        CancellationToken cancellationToken = default)
    {
        // Invalidate all tenant-related cache entries
        var pattern = $"tenant:{tenantId}:*";
        
        _logger.LogInformation(
            "Tenant {TenantId} cache invalidated. Reason: {Reason}",
            tenantId, reason);

        return RemoveByPatternAsync(pattern, reason, cancellationToken);
    }

    public Task<CacheStatistics> GetStatisticsAsync(CancellationToken cancellationToken = default)
    {
        var totalMemory = _cache.Values.Sum(e => EstimateSize(e.Value));
        
        var stats = new CacheStatistics
        {
            TotalItems = _cache.Count,
            TotalMemoryBytes = totalMemory,
            HitCount = _hitCount,
            MissCount = _missCount,
            InvalidationsByReason = new Dictionary<CacheInvalidationReason, int>(_invalidationCounts)
        };

        return Task.FromResult(stats);
    }

    private static bool MatchesPattern(string key, string pattern)
    {
        // Simple wildcard matching (supports * at end)
        if (pattern.EndsWith('*'))
        {
            var prefix = pattern[..^1];
            return key.StartsWith(prefix, StringComparison.OrdinalIgnoreCase);
        }

        return key.Equals(pattern, StringComparison.OrdinalIgnoreCase);
    }

    private void IncrementInvalidationCount(CacheInvalidationReason reason)
    {
        _invalidationCounts.AddOrUpdate(reason, 1, (_, count) => count + 1);
    }

    private static long EstimateSize(object value)
    {
        // Simple estimation - in production, use more accurate measurement
        if (value is string str)
        {
            return str.Length * 2; // Unicode characters
        }

        return 100; // Default estimate for objects
    }

    private async Task CleanupExpiredEntriesAsync()
    {
        while (true)
        {
            try
            {
                await Task.Delay(TimeSpan.FromMinutes(5));

                var expiredKeys = _cache
                    .Where(kvp => kvp.Value.IsExpired)
                    .Select(kvp => kvp.Key)
                    .ToList();

                foreach (var key in expiredKeys)
                {
                    if (_cache.TryRemove(key, out _))
                    {
                        IncrementInvalidationCount(CacheInvalidationReason.Expiration);
                    }
                }

                if (expiredKeys.Any())
                {
                    _logger.LogDebug("Cleaned up {Count} expired cache entries", expiredKeys.Count);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during cache cleanup");
            }
        }
    }

    private sealed class CacheEntry
    {
        public object Value { get; init; } = default!;
        public DateTime ExpiresAt { get; init; }
        public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
    }
}
