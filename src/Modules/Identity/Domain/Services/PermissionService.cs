using WebSearchIndexing.Modules.Identity.Domain.Constants;

namespace WebSearchIndexing.Modules.Identity.Domain.Services;

/// <summary>
/// Service for permission validation and management
/// </summary>
public static class PermissionService
{
    /// <summary>
    /// Checks if user has specific permission based on their roles
    /// </summary>
    /// <param name="userPermissions">Comma-separated user permissions</param>
    /// <param name="requiredPermission">Permission to check</param>
    /// <returns>True if user has permission, false otherwise</returns>
    public static bool HasPermission(string? userPermissions, string requiredPermission)
    {
        if (string.IsNullOrWhiteSpace(userPermissions) || string.IsNullOrWhiteSpace(requiredPermission))
            return false;

        var permissions = userPermissions.Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(p => p.Trim())
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        return permissions.Contains(requiredPermission);
    }

    /// <summary>
    /// Checks if user has any of the specified permissions
    /// </summary>
    /// <param name="userPermissions">Comma-separated user permissions</param>
    /// <param name="requiredPermissions">Permissions to check (any one is sufficient)</param>
    /// <returns>True if user has at least one permission, false otherwise</returns>
    public static bool HasAnyPermission(string? userPermissions, params string[] requiredPermissions)
    {
        if (string.IsNullOrWhiteSpace(userPermissions) || requiredPermissions?.Length == 0)
            return false;

        return requiredPermissions?.Any(permission => HasPermission(userPermissions, permission)) ?? false;
    }

    /// <summary>
    /// Checks if user has all of the specified permissions
    /// </summary>
    /// <param name="userPermissions">Comma-separated user permissions</param>
    /// <param name="requiredPermissions">Permissions to check (all must be present)</param>
    /// <returns>True if user has all permissions, false otherwise</returns>
    public static bool HasAllPermissions(string? userPermissions, params string[] requiredPermissions)
    {
        if (string.IsNullOrWhiteSpace(userPermissions) || requiredPermissions?.Length == 0)
            return false;

        return requiredPermissions?.All(permission => HasPermission(userPermissions, permission)) ?? false;
    }

    /// <summary>
    /// Gets all permissions for a specific role type
    /// </summary>
    /// <param name="roleName">Name of the role</param>
    /// <returns>Array of permissions for the role</returns>
    public static string[] GetRolePermissions(string roleName)
    {
        return roleName switch
        {
            Roles.GlobalAdmin => Permissions.Groups.GlobalAdminPermissions,
            Roles.TenantAdmin => Permissions.Groups.TenantAdminPermissions,
            Roles.TenantUser => Permissions.Groups.TenantUserPermissions,
            Roles.DomainAdmin => Permissions.Groups.DomainAdminPermissions,
            Roles.DomainUser => Permissions.Groups.DomainUserPermissions,
            _ => Array.Empty<string>()
        };
    }

    /// <summary>
    /// Validates permission string format
    /// </summary>
    /// <param name="permissionString">Comma-separated permission string</param>
    /// <returns>Validation result with details</returns>
    public static PermissionValidationResult ValidatePermissionString(string? permissionString)
    {
        if (string.IsNullOrWhiteSpace(permissionString))
        {
            return new PermissionValidationResult(false, "Permission string cannot be empty");
        }

        var permissions = permissionString.Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(p => p.Trim())
            .ToArray();

        var invalidPermissions = permissions.Where(p => !Permissions.IsValid(p)).ToArray();

        if (invalidPermissions.Length > 0)
        {
            return new PermissionValidationResult(false, 
                $"Invalid permissions: {string.Join(", ", invalidPermissions)}");
        }

        return new PermissionValidationResult(true, "All permissions are valid");
    }

    /// <summary>
    /// Merges multiple permission strings into one
    /// </summary>
    /// <param name="permissionStrings">Permission strings to merge</param>
    /// <returns>Merged permission string with duplicates removed</returns>
    public static string MergePermissions(params string[] permissionStrings)
    {
        var allPermissions = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var permissionString in permissionStrings.Where(ps => !string.IsNullOrWhiteSpace(ps)))
        {
            var permissions = permissionString.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(p => p.Trim());

            foreach (var permission in permissions)
            {
                allPermissions.Add(permission);
            }
        }

        return string.Join(",", allPermissions.OrderBy(p => p));
    }
}

/// <summary>
/// Result of permission validation
/// </summary>
/// <param name="IsValid">Whether the validation passed</param>
/// <param name="Message">Validation message</param>
public record PermissionValidationResult(bool IsValid, string Message);
