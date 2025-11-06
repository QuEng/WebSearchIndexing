using WebSearchIndexing.Modules.Identity.Application.Caching;
using WebSearchIndexing.Modules.Identity.Domain.Entities;
using WebSearchIndexing.Modules.Identity.Domain.Repositories;

namespace WebSearchIndexing.Modules.Identity.Infrastructure.Caching;

/// <summary>
/// Cached decorator for IUserRepository with security-aware invalidation
/// </summary>
public sealed class CachedUserRepository : IUserRepository
{
    private readonly IUserRepository _inner;
    private readonly ICacheService _cache;
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(15); // Match token lifetime

    public CachedUserRepository(IUserRepository inner, ICacheService cache)
    {
        _inner = inner ?? throw new ArgumentNullException(nameof(inner));
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
    }

    public async Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var key = $"user:{id}:entity";
        var cached = await _cache.GetAsync<User>(key, cancellationToken);
        if (cached != null)
            return cached;

        var user = await _inner.GetByIdAsync(id, cancellationToken);
        if (user != null)
            await _cache.SetAsync(key, user, CacheDuration, cancellationToken);

        return user;
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var key = $"user:email:{email.ToLowerInvariant()}";
        var cached = await _cache.GetAsync<User>(key, cancellationToken);
        if (cached != null)
            return cached;

        var user = await _inner.GetByEmailAsync(email, cancellationToken);
        if (user != null)
            await _cache.SetAsync(key, user, CacheDuration, cancellationToken);

        return user;
    }

    public Task<IReadOnlyCollection<User>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        // Don't cache collection queries - too dynamic
        return _inner.GetAllAsync(cancellationToken);
    }

    public Task<IEnumerable<User>> GetByTenantAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        // Don't cache tenant queries - too dynamic
        return _inner.GetByTenantAsync(tenantId, cancellationToken);
    }

    public Task<IReadOnlyCollection<User>> GetByDomainAsync(Guid domainId, CancellationToken cancellationToken = default)
    {
        // Don't cache domain queries - too dynamic
        return _inner.GetByDomainAsync(domainId, cancellationToken);
    }

    public Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return _inner.ExistsByEmailAsync(email, cancellationToken);
    }

    public async Task<IReadOnlyCollection<string>> GetUserGlobalRolesAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var key = $"user:{userId}:globalroles";
        return await _cache.GetOrCreateAsync(
            key,
            () => _inner.GetUserGlobalRolesAsync(userId, cancellationToken),
            CacheDuration,
            cancellationToken) ?? Array.Empty<string>();
    }

    public Task<(IEnumerable<User> Users, int TotalCount)> GetPagedAsync(
        int page,
        int pageSize,
        string? searchTerm = null,
        CancellationToken cancellationToken = default)
    {
        // Don't cache paged queries - too dynamic
        return _inner.GetPagedAsync(page, pageSize, searchTerm, cancellationToken);
    }

    public async Task<User> AddAsync(User user, CancellationToken cancellationToken = default)
    {
        var result = await _inner.AddAsync(user, cancellationToken);
        
        // Invalidate user cache on create
        await _cache.InvalidateUserCacheAsync(user.Id, CacheInvalidationReason.ManualInvalidation, cancellationToken);
        
        return result;
    }

    public async Task<User> UpdateAsync(User user, CancellationToken cancellationToken = default)
    {
        var result = await _inner.UpdateAsync(user, cancellationToken);
        
        // Invalidate user cache on update
        await _cache.InvalidateUserCacheAsync(user.Id, CacheInvalidationReason.ManualInvalidation, cancellationToken);
        
        return result;
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await _inner.DeleteAsync(id, cancellationToken);
        
        // Invalidate user cache on delete
        await _cache.InvalidateUserCacheAsync(id, CacheInvalidationReason.ManualInvalidation, cancellationToken);
    }
}
