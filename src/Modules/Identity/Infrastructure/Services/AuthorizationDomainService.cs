using WebSearchIndexing.Modules.Identity.Domain.Constants;
using WebSearchIndexing.Modules.Identity.Domain.Repositories;
using WebSearchIndexing.Modules.Identity.Domain.Services;

namespace WebSearchIndexing.Modules.Identity.Infrastructure.Services;

/// <summary>
/// Implementation of authorization domain service
/// </summary>
public sealed class AuthorizationDomainService : IAuthorizationDomainService
{
    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;

    public AuthorizationDomainService(
        IUserRepository userRepository,
        IRoleRepository roleRepository)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _roleRepository = roleRepository ?? throw new ArgumentNullException(nameof(roleRepository));
    }

    public async Task<bool> HasGlobalPermissionAsync(Guid userId, string permission, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(permission);

        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user == null || !user.IsActive)
            return false;

        // Check if user has any global roles with this permission
        var globalRoles = await _roleRepository.GetGlobalRolesAsync(cancellationToken);
        var userGlobalRoles = await _userRepository.GetUserGlobalRolesAsync(userId, cancellationToken);

        foreach (var roleName in userGlobalRoles)
        {
            var role = globalRoles.FirstOrDefault(r => r.Name == roleName && r.IsActive);
            if (role?.HasPermission(permission) == true)
                return true;
        }

        return false;
    }

    public async Task<bool> HasTenantPermissionAsync(Guid userId, Guid tenantId, string permission, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(permission);

        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user == null || !user.IsActive)
            return false;

        // First check global permissions (global admin can do anything)
        if (await HasGlobalPermissionAsync(userId, PermissionConstants.Global.ManageAllTenants, cancellationToken))
            return true;

        // Check tenant-specific roles
        var userTenant = user.UserTenants.FirstOrDefault(ut => ut.TenantId == tenantId && ut.IsActive);
        if (userTenant == null)
            return false;

        var tenantRoles = await _roleRepository.GetTenantRolesAsync(cancellationToken);
        var role = tenantRoles.FirstOrDefault(r => r.Name == userTenant.Role && r.IsActive);

        return role?.HasPermission(permission) == true;
    }

    public async Task<bool> HasDomainPermissionAsync(Guid userId, Guid domainId, string permission, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(permission);

        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user == null || !user.IsActive)
            return false;

        // First check global permissions
        if (await HasGlobalPermissionAsync(userId, PermissionConstants.Global.ManageAllTenants, cancellationToken))
            return true;

        // Check domain-specific roles
        var userDomain = user.UserDomains.FirstOrDefault(ud => ud.DomainId == domainId && ud.IsActive);
        if (userDomain == null)
            return false;

        var domainRoles = await _roleRepository.GetDomainRolesAsync(cancellationToken);
        var role = domainRoles.FirstOrDefault(r => r.Name == userDomain.Role && r.IsActive);

        return role?.HasPermission(permission) == true;
    }

    public async Task<IEnumerable<string>> GetUserTenantPermissionsAsync(Guid userId, Guid tenantId, CancellationToken cancellationToken = default)
    {
        var permissions = new HashSet<string>();

        // Add global permissions
        var globalPermissions = await GetUserGlobalPermissionsAsync(userId, cancellationToken);
        foreach (var permission in globalPermissions)
        {
            permissions.Add(permission);
        }

        // Add tenant permissions
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user?.IsActive == true)
        {
            var userTenant = user.UserTenants.FirstOrDefault(ut => ut.TenantId == tenantId && ut.IsActive);
            if (userTenant != null)
            {
                var tenantRoles = await _roleRepository.GetTenantRolesAsync(cancellationToken);
                var role = tenantRoles.FirstOrDefault(r => r.Name == userTenant.Role && r.IsActive);
                if (role != null)
                {
                    var rolePermissions = role.Permissions.Split(',', StringSplitOptions.RemoveEmptyEntries);
                    foreach (var permission in rolePermissions)
                    {
                        permissions.Add(permission.Trim());
                    }
                }
            }
        }

        return permissions;
    }

    public async Task<IEnumerable<string>> GetUserDomainPermissionsAsync(Guid userId, Guid domainId, CancellationToken cancellationToken = default)
    {
        var permissions = new HashSet<string>();

        // Add global permissions
        var globalPermissions = await GetUserGlobalPermissionsAsync(userId, cancellationToken);
        foreach (var permission in globalPermissions)
        {
            permissions.Add(permission);
        }

        // Add domain permissions
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user?.IsActive == true)
        {
            var userDomain = user.UserDomains.FirstOrDefault(ud => ud.DomainId == domainId && ud.IsActive);
            if (userDomain != null)
            {
                var domainRoles = await _roleRepository.GetDomainRolesAsync(cancellationToken);
                var role = domainRoles.FirstOrDefault(r => r.Name == userDomain.Role && r.IsActive);
                if (role != null)
                {
                    var rolePermissions = role.Permissions.Split(',', StringSplitOptions.RemoveEmptyEntries);
                    foreach (var permission in rolePermissions)
                    {
                        permissions.Add(permission.Trim());
                    }
                }
            }
        }

        return permissions;
    }

    public async Task<IEnumerable<string>> GetUserGlobalPermissionsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var permissions = new HashSet<string>();

        var globalRoles = await _roleRepository.GetGlobalRolesAsync(cancellationToken);
        var userGlobalRoles = await _userRepository.GetUserGlobalRolesAsync(userId, cancellationToken);

        foreach (var roleName in userGlobalRoles)
        {
            var role = globalRoles.FirstOrDefault(r => r.Name == roleName && r.IsActive);
            if (role != null)
            {
                var rolePermissions = role.Permissions.Split(',', StringSplitOptions.RemoveEmptyEntries);
                foreach (var permission in rolePermissions)
                {
                    permissions.Add(permission.Trim());
                }
            }
        }

        return permissions;
    }

    public async Task<bool> CanAccessResourceAsync(Guid userId, string resourceType, Guid resourceId, string permission, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(resourceType);
        ArgumentNullException.ThrowIfNull(permission);

        return resourceType.ToLowerInvariant() switch
        {
            "tenant" => await HasTenantPermissionAsync(userId, resourceId, permission, cancellationToken),
            "domain" => await HasDomainPermissionAsync(userId, resourceId, permission, cancellationToken),
            "global" => await HasGlobalPermissionAsync(userId, permission, cancellationToken),
            _ => false
        };
    }
}
