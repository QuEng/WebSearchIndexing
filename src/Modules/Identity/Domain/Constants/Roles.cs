using WebSearchIndexing.Modules.Identity.Domain.Entities;

namespace WebSearchIndexing.Modules.Identity.Domain.Constants;

/// <summary>
/// Static class containing all role name constants for the Identity system
/// </summary>
public static class Roles
{
    // Global roles
    public const string GlobalAdmin = "GlobalAdmin";

    // Tenant roles
    public const string TenantAdmin = "TenantAdmin";
    public const string TenantUser = "TenantUser";
    public const string Owner = "Owner";
    public const string Admin = "Admin";
    public const string Member = "Member";
    public const string Viewer = "Viewer";

    // Domain roles
    public const string DomainAdmin = "DomainAdmin";
    public const string DomainEditor = "DomainEditor";
    public const string DomainUser = "DomainUser";
    public const string DomainViewer = "DomainViewer";

    /// <summary>
    /// Role validation helpers
    /// </summary>
    public static class Validation
    {
        public static readonly string[] GlobalRoles = new[]
        {
            GlobalAdmin
        };

        public static readonly string[] TenantRoles = new[]
        {
            TenantAdmin, TenantUser, Owner, Admin, Member, Viewer
        };

        public static readonly string[] DomainRoles = new[]
        {
            DomainAdmin, DomainEditor, DomainUser, DomainViewer
        };

        public static readonly string[] AllRoles = GlobalRoles
            .Concat(TenantRoles)
            .Concat(DomainRoles)
            .ToArray();

        /// <summary>
        /// Validates if a role name is valid
        /// </summary>
        /// <param name="roleName">Role name to validate</param>
        /// <returns>True if valid, false otherwise</returns>
        public static bool IsValid(string roleName)
        {
            return AllRoles.Contains(roleName, StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Validates if a role belongs to specific type
        /// </summary>
        /// <param name="roleName">Role name to validate</param>
        /// <param name="roleType">Expected role type</param>
        /// <returns>True if role belongs to type, false otherwise</returns>
        public static bool IsOfType(string roleName, RoleType roleType)
        {
            return roleType switch
            {
                RoleType.Global => GlobalRoles.Contains(roleName, StringComparer.OrdinalIgnoreCase),
                RoleType.Tenant => TenantRoles.Contains(roleName, StringComparer.OrdinalIgnoreCase),
                RoleType.Domain => DomainRoles.Contains(roleName, StringComparer.OrdinalIgnoreCase),
                _ => false
            };
        }
    }

    /// <summary>
    /// Role hierarchy helpers for permission checks
    /// </summary>
    public static class Hierarchy
    {
        /// <summary>
        /// Checks if role has administrative privileges at tenant level
        /// </summary>
        /// <param name="roleName">Role name to check</param>
        /// <returns>True if role has admin privileges, false otherwise</returns>
        public static bool IsTenantAdmin(string roleName)
        {
            return roleName?.Equals(TenantAdmin, StringComparison.OrdinalIgnoreCase) == true ||
                   roleName?.Equals(Owner, StringComparison.OrdinalIgnoreCase) == true ||
                   roleName?.Equals(Admin, StringComparison.OrdinalIgnoreCase) == true;
        }

        /// <summary>
        /// Checks if role has administrative privileges at domain level
        /// </summary>
        /// <param name="roleName">Role name to check</param>
        /// <returns>True if role has admin privileges, false otherwise</returns>
        public static bool IsDomainAdmin(string roleName)
        {
            return roleName?.Equals(DomainAdmin, StringComparison.OrdinalIgnoreCase) == true;
        }

        /// <summary>
        /// Checks if role can manage other users
        /// </summary>
        /// <param name="roleName">Role name to check</param>
        /// <returns>True if role can manage users, false otherwise</returns>
        public static bool CanManageUsers(string? roleName)
        {
            return roleName?.Equals(GlobalAdmin, StringComparison.OrdinalIgnoreCase) == true ||
                   (roleName != null && IsTenantAdmin(roleName));
        }

        /// <summary>
        /// Checks if role can manage domains
        /// </summary>
        /// <param name="roleName">Role name to check</param>
        /// <returns>True if role can manage domains, false otherwise</returns>
        public static bool CanManageDomains(string roleName)
        {
            return IsTenantAdmin(roleName) || IsDomainAdmin(roleName);
        }
    }
}
