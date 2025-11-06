using WebSearchIndexing.Modules.Identity.Domain.Entities;

namespace WebSearchIndexing.Modules.Identity.Domain.Constants;

/// <summary>
/// Predefined roles in the system with their permissions
/// </summary>
public static class RoleConstants
{
    // Global Roles
    public static class Global
    {
        public const string GlobalAdmin = "GlobalAdmin";
        public const string SystemModerator = "SystemModerator";

        public static readonly Dictionary<string, string[]> RolePermissions = new()
        {
            [GlobalAdmin] = new[]
            {
                PermissionConstants.Global.ManageSystem,
                PermissionConstants.Global.ManageAllTenants,
                PermissionConstants.Global.ViewAllTenants,
                PermissionConstants.Global.ManageGlobalUsers,
                PermissionConstants.Global.ViewSystemMetrics,
                PermissionConstants.Global.ManageSystemSettings
            },
            [SystemModerator] = new[]
            {
                PermissionConstants.Global.ViewAllTenants,
                PermissionConstants.Global.ViewSystemMetrics
            }
        };
    }

    // Tenant Roles
    public static class Tenant
    {
        public const string Owner = "Owner";
        public const string Admin = "Admin";
        public const string Member = "Member";
        public const string Viewer = "Viewer";

        public static readonly Dictionary<string, string[]> RolePermissions = new()
        {
            [Owner] = new[]
            {
                PermissionConstants.Tenant.ManageTenant,
                PermissionConstants.Tenant.ViewTenantSettings,
                PermissionConstants.Tenant.ManageTenantUsers,
                PermissionConstants.Tenant.InviteUsers,
                PermissionConstants.Tenant.ManageRoles,
                PermissionConstants.Tenant.ViewTenantMetrics,
                PermissionConstants.Tenant.ManageDomains,
                PermissionConstants.Tenant.ViewDomains
            },
            [Admin] = new[]
            {
                PermissionConstants.Tenant.ViewTenantSettings,
                PermissionConstants.Tenant.ManageTenantUsers,
                PermissionConstants.Tenant.InviteUsers,
                PermissionConstants.Tenant.ViewTenantMetrics,
                PermissionConstants.Tenant.ManageDomains,
                PermissionConstants.Tenant.ViewDomains
            },
            [Member] = new[]
            {
                PermissionConstants.Tenant.ViewTenantSettings,
                PermissionConstants.Tenant.ViewTenantMetrics,
                PermissionConstants.Tenant.ViewDomains
            },
            [Viewer] = new[]
            {
                PermissionConstants.Tenant.ViewTenantSettings,
                PermissionConstants.Tenant.ViewDomains
            }
        };
    }

    // Domain Roles
    public static class Domain
    {
        public const string DomainAdmin = "DomainAdmin";
        public const string DomainEditor = "DomainEditor";
        public const string DomainViewer = "DomainViewer";

        public static readonly Dictionary<string, string[]> RolePermissions = new()
        {
            [DomainAdmin] = new[]
            {
                PermissionConstants.Domain.ManageDomain,
                PermissionConstants.Domain.ViewDomainSettings,
                PermissionConstants.Domain.ManageDomainUsers,
                PermissionConstants.Domain.SubmitUrls,
                PermissionConstants.Domain.ViewUrls,
                PermissionConstants.Domain.ManageUrlBatches,
                PermissionConstants.Domain.ViewReports,
                PermissionConstants.Domain.ManageServiceAccounts,
                PermissionConstants.Domain.ViewServiceAccounts
            },
            [DomainEditor] = new[]
            {
                PermissionConstants.Domain.ViewDomainSettings,
                PermissionConstants.Domain.SubmitUrls,
                PermissionConstants.Domain.ViewUrls,
                PermissionConstants.Domain.ManageUrlBatches,
                PermissionConstants.Domain.ViewReports,
                PermissionConstants.Domain.ViewServiceAccounts
            },
            [DomainViewer] = new[]
            {
                PermissionConstants.Domain.ViewDomainSettings,
                PermissionConstants.Domain.ViewUrls,
                PermissionConstants.Domain.ViewReports,
                PermissionConstants.Domain.ViewServiceAccounts
            }
        };
    }

    /// <summary>
    /// Gets all predefined roles
    /// </summary>
    public static IEnumerable<string> GetAllRoles()
    {
        return Global.RolePermissions.Keys
            .Concat(Tenant.RolePermissions.Keys)
            .Concat(Domain.RolePermissions.Keys);
    }

    /// <summary>
    /// Gets permissions for a specific role
    /// </summary>
    public static string[] GetRolePermissions(string roleName)
    {
        return Global.RolePermissions.GetValueOrDefault(roleName)
            ?? Tenant.RolePermissions.GetValueOrDefault(roleName)
            ?? Domain.RolePermissions.GetValueOrDefault(roleName)
            ?? Array.Empty<string>();
    }

    /// <summary>
    /// Validates if a role is predefined in the system
    /// </summary>
    public static bool IsValidRole(string roleName)
    {
        return GetAllRoles().Contains(roleName);
    }

    /// <summary>
    /// Gets role type based on role name
    /// </summary>
    public static RoleType GetRoleType(string roleName)
    {
        if (Global.RolePermissions.ContainsKey(roleName))
            return RoleType.Global;
        if (Tenant.RolePermissions.ContainsKey(roleName))
            return RoleType.Tenant;
        if (Domain.RolePermissions.ContainsKey(roleName))
            return RoleType.Domain;

        throw new ArgumentException($"Unknown role: {roleName}");
    }
}
