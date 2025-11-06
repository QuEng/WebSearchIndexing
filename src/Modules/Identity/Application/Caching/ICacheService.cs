namespace WebSearchIndexing.Modules.Identity.Application.Caching;

/// <summary>
/// Cache invalidation reasons for security tracking
/// </summary>
public enum CacheInvalidationReason
{
    TokenRefresh,
    UserLogout,
    PasswordChange,
    RoleChange,
    PermissionChange,
    SecurityPolicyUpdate,
    ManualInvalidation,
    Expiration
}

/// <summary>
/// Service for caching with security-aware invalidation
/// </summary>
public interface ICacheService
{
    /// <summary>
    /// Gets cached item or creates it using factory
    /// </summary>
    Task<T?> GetOrCreateAsync<T>(
        string key,
        Func<Task<T>> factory,
        TimeSpan? expiration = null,
        CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Gets cached item
    /// </summary>
    Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Sets cached item
    /// </summary>
    Task SetAsync<T>(
        string key,
        T value,
        TimeSpan? expiration = null,
        CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Removes cached item with security tracking
    /// </summary>
    Task RemoveAsync(
        string key,
        CacheInvalidationReason reason = CacheInvalidationReason.ManualInvalidation,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes all cached items matching pattern with security tracking
    /// </summary>
    Task RemoveByPatternAsync(
        string pattern,
        CacheInvalidationReason reason = CacheInvalidationReason.ManualInvalidation,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Invalidates all user-related cache on token refresh
    /// </summary>
    Task InvalidateUserCacheAsync(
        Guid userId,
        CacheInvalidationReason reason = CacheInvalidationReason.TokenRefresh,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Invalidates all tenant-related cache
    /// </summary>
    Task InvalidateTenantCacheAsync(
        Guid tenantId,
        CacheInvalidationReason reason = CacheInvalidationReason.SecurityPolicyUpdate,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets cache statistics
    /// </summary>
    Task<CacheStatistics> GetStatisticsAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Cache statistics for monitoring
/// </summary>
public sealed class CacheStatistics
{
    public int TotalItems { get; init; }
    public long TotalMemoryBytes { get; init; }
    public int HitCount { get; init; }
    public int MissCount { get; init; }
    public double HitRate => HitCount + MissCount > 0 
        ? (double)HitCount / (HitCount + MissCount) 
        : 0;
    public Dictionary<CacheInvalidationReason, int> InvalidationsByReason { get; init; } = new();
}
