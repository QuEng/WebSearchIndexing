namespace WebSearchIndexing.Modules.Identity.Domain.Constants;

/// <summary>
/// Static class containing all permission constants for the Identity system
/// </summary>
public static class Permissions
{
    // User permissions
    public const string UserRead = "user:read";
    public const string UserWrite = "user:write";
    public const string UserDelete = "user:delete";

    // Tenant permissions
    public const string TenantRead = "tenant:read";
    public const string TenantWrite = "tenant:write";
    public const string TenantDelete = "tenant:delete";

    // Role permissions
    public const string RoleRead = "role:read";
    public const string RoleWrite = "role:write";
    public const string RoleDelete = "role:delete";

    // Domain permissions
    public const string DomainRead = "domain:read";
    public const string DomainWrite = "domain:write";
    public const string DomainDelete = "domain:delete";

    // Indexing permissions
    public const string IndexingRead = "indexing:read";
    public const string IndexingWrite = "indexing:write";

    // Permission groups for convenience
    public static class Groups
    {
        public static readonly string[] GlobalAdminPermissions = new[]
        {
            UserRead, UserWrite, UserDelete,
            TenantRead, TenantWrite, TenantDelete,
            RoleRead, RoleWrite, RoleDelete
        };

        public static readonly string[] TenantAdminPermissions = new[]
        {
            UserRead, UserWrite,
            DomainRead, DomainWrite, DomainDelete
        };

        public static readonly string[] TenantUserPermissions = new[]
        {
            UserRead,
            DomainRead
        };

        public static readonly string[] DomainAdminPermissions = new[]
        {
            DomainRead, DomainWrite,
            IndexingRead, IndexingWrite
        };

        public static readonly string[] DomainUserPermissions = new[]
        {
            DomainRead,
            IndexingRead
        };
    }

    /// <summary>
    /// Converts permission array to comma-separated string
    /// </summary>
    /// <param name="permissions">Array of permissions</param>
    /// <returns>Comma-separated permission string</returns>
    public static string Join(params string[] permissions)
    {
        return string.Join(",", permissions);
    }

    /// <summary>
    /// Validates if a permission string is valid
    /// </summary>
    /// <param name="permission">Permission to validate</param>
    /// <returns>True if valid, false otherwise</returns>
    public static bool IsValid(string permission)
    {
        var allPermissions = new[]
        {
            UserRead, UserWrite, UserDelete,
            TenantRead, TenantWrite, TenantDelete,
            RoleRead, RoleWrite, RoleDelete,
            DomainRead, DomainWrite, DomainDelete,
            IndexingRead, IndexingWrite
        };

        return allPermissions.Contains(permission);
    }
}
