using WebSearchIndexing.Modules.Identity.Application.Caching;
using WebSearchIndexing.Modules.Identity.Domain.Entities;
using WebSearchIndexing.Modules.Identity.Domain.Repositories;

namespace WebSearchIndexing.Modules.Identity.Infrastructure.Caching;

/// <summary>
/// Cached decorator for IRoleRepository with security-aware invalidation
/// </summary>
public sealed class CachedRoleRepository : IRoleRepository
{
    private readonly IRoleRepository _inner;
    private readonly ICacheService _cache;
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(30); // Roles change infrequently

    public CachedRoleRepository(IRoleRepository inner, ICacheService cache)
    {
        _inner = inner ?? throw new ArgumentNullException(nameof(inner));
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
    }

    public async Task<Role?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var key = $"role:{id}:entity";
        var cached = await _cache.GetAsync<Role>(key, cancellationToken);
        if (cached != null)
            return cached;

        var role = await _inner.GetByIdAsync(id, cancellationToken);
        if (role != null)
            await _cache.SetAsync(key, role, CacheDuration, cancellationToken);

        return role;
    }

    public async Task<Role?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        var key = $"role:name:{name.ToLowerInvariant()}";
        var cached = await _cache.GetAsync<Role>(key, cancellationToken);
        if (cached != null)
            return cached;

        var role = await _inner.GetByNameAsync(name, cancellationToken);
        if (role != null)
            await _cache.SetAsync(key, role, CacheDuration, cancellationToken);

        return role;
    }

    public async Task<IReadOnlyCollection<Role>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var key = "role:all";
        return await _cache.GetOrCreateAsync(
            key,
            () => _inner.GetAllAsync(cancellationToken),
            CacheDuration,
            cancellationToken) ?? Array.Empty<Role>();
    }

    public async Task<IReadOnlyCollection<Role>> GetByTypeAsync(RoleType type, CancellationToken cancellationToken = default)
    {
        var key = $"role:type:{type}";
        return await _cache.GetOrCreateAsync(
            key,
            () => _inner.GetByTypeAsync(type, cancellationToken),
            CacheDuration,
            cancellationToken) ?? Array.Empty<Role>();
    }

    public async Task<IReadOnlyCollection<Role>> GetGlobalRolesAsync(CancellationToken cancellationToken = default)
    {
        var key = "role:global";
        return await _cache.GetOrCreateAsync(
            key,
            () => _inner.GetGlobalRolesAsync(cancellationToken),
            CacheDuration,
            cancellationToken) ?? Array.Empty<Role>();
    }

    public async Task<IReadOnlyCollection<Role>> GetTenantRolesAsync(CancellationToken cancellationToken = default)
    {
        var key = "role:tenant";
        return await _cache.GetOrCreateAsync(
            key,
            () => _inner.GetTenantRolesAsync(cancellationToken),
            CacheDuration,
            cancellationToken) ?? Array.Empty<Role>();
    }

    public async Task<IReadOnlyCollection<Role>> GetDomainRolesAsync(CancellationToken cancellationToken = default)
    {
        var key = "role:domain";
        return await _cache.GetOrCreateAsync(
            key,
            () => _inner.GetDomainRolesAsync(cancellationToken),
            CacheDuration,
            cancellationToken) ?? Array.Empty<Role>();
    }

    public Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        return _inner.ExistsByNameAsync(name, cancellationToken);
    }

    public async Task<Role> AddAsync(Role role, CancellationToken cancellationToken = default)
    {
        var result = await _inner.AddAsync(role, cancellationToken);
        
        // Invalidate all roles cache
        await _cache.RemoveAsync("role:all", CacheInvalidationReason.PermissionChange, cancellationToken);
        
        return result;
    }

    public async Task<Role> UpdateAsync(Role role, CancellationToken cancellationToken = default)
    {
        var result = await _inner.UpdateAsync(role, cancellationToken);
        
        // Invalidate role and all roles cache
        await _cache.RemoveAsync($"role:{role.Id}:entity", CacheInvalidationReason.PermissionChange, cancellationToken);
        await _cache.RemoveAsync($"role:name:{role.Name.ToLowerInvariant()}", CacheInvalidationReason.PermissionChange, cancellationToken);
        await _cache.RemoveAsync("role:all", CacheInvalidationReason.PermissionChange, cancellationToken);
        
        return result;
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await _inner.DeleteAsync(id, cancellationToken);
        
        // Invalidate role cache
        await _cache.RemoveAsync($"role:{id}:entity", CacheInvalidationReason.PermissionChange, cancellationToken);
        await _cache.RemoveAsync("role:all", CacheInvalidationReason.PermissionChange, cancellationToken);
    }
}
