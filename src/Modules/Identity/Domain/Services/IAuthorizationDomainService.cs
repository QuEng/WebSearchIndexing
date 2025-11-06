using WebSearchIndexing.Modules.Identity.Domain.Constants;
using WebSearchIndexing.Modules.Identity.Domain.Entities;
using WebSearchIndexing.Modules.Identity.Domain.Repositories;

namespace WebSearchIndexing.Modules.Identity.Domain.Services;

/// <summary>
/// Domain service for authorization logic
/// </summary>
public interface IAuthorizationDomainService
{
    /// <summary>
    /// Checks if user has global permission
    /// </summary>
    Task<bool> HasGlobalPermissionAsync(Guid userId, string permission, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if user has tenant permission
    /// </summary>
    Task<bool> HasTenantPermissionAsync(Guid userId, Guid tenantId, string permission, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if user has domain permission
    /// </summary>
    Task<bool> HasDomainPermissionAsync(Guid userId, Guid domainId, string permission, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all user permissions for a specific tenant
    /// </summary>
    Task<IEnumerable<string>> GetUserTenantPermissionsAsync(Guid userId, Guid tenantId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all user permissions for a specific domain
    /// </summary>
    Task<IEnumerable<string>> GetUserDomainPermissionsAsync(Guid userId, Guid domainId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all global permissions for user
    /// </summary>
    Task<IEnumerable<string>> GetUserGlobalPermissionsAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates if user can access resource
    /// </summary>
    Task<bool> CanAccessResourceAsync(Guid userId, string resourceType, Guid resourceId, string permission, CancellationToken cancellationToken = default);
}
