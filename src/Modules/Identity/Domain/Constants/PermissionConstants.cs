using System.Collections.ObjectModel;

namespace WebSearchIndexing.Modules.Identity.Domain.Constants;

/// <summary>
/// Defines all available permissions in the system
/// </summary>
public static class PermissionConstants
{
    // Global permissions
    public static class Global
    {
        public const string ManageSystem = "global.manage_system";
        public const string ManageAllTenants = "global.manage_all_tenants";
        public const string ViewAllTenants = "global.view_all_tenants";
        public const string ManageGlobalUsers = "global.manage_global_users";
        public const string ViewSystemMetrics = "global.view_system_metrics";
        public const string ManageSystemSettings = "global.manage_system_settings";
    }

    // Tenant permissions
    public static class Tenant
    {
        public const string ManageTenant = "tenant.manage_tenant";
        public const string ViewTenantSettings = "tenant.view_tenant_settings";
        public const string ManageTenantUsers = "tenant.manage_tenant_users";
        public const string InviteUsers = "tenant.invite_users";
        public const string ManageRoles = "tenant.manage_roles";
        public const string ViewTenantMetrics = "tenant.view_tenant_metrics";
        public const string ManageDomains = "tenant.manage_domains";
        public const string ViewDomains = "tenant.view_domains";
    }

    // Domain permissions
    public static class Domain
    {
        public const string ManageDomain = "domain.manage_domain";
        public const string ViewDomainSettings = "domain.view_domain_settings";
        public const string ManageDomainUsers = "domain.manage_domain_users";
        public const string SubmitUrls = "domain.submit_urls";
        public const string ViewUrls = "domain.view_urls";
        public const string ManageUrlBatches = "domain.manage_url_batches";
        public const string ViewReports = "domain.view_reports";
        public const string ManageServiceAccounts = "domain.manage_service_accounts";
        public const string ViewServiceAccounts = "domain.view_service_accounts";
    }

    // All permissions for validation
    public static readonly ReadOnlyCollection<string> AllPermissions = new(new[]
    {
        // Global
        Global.ManageSystem,
        Global.ManageAllTenants,
        Global.ViewAllTenants,
        Global.ManageGlobalUsers,
        Global.ViewSystemMetrics,
        Global.ManageSystemSettings,

        // Tenant
        Tenant.ManageTenant,
        Tenant.ViewTenantSettings,
        Tenant.ManageTenantUsers,
        Tenant.InviteUsers,
        Tenant.ManageRoles,
        Tenant.ViewTenantMetrics,
        Tenant.ManageDomains,
        Tenant.ViewDomains,

        // Domain
        Domain.ManageDomain,
        Domain.ViewDomainSettings,
        Domain.ManageDomainUsers,
        Domain.SubmitUrls,
        Domain.ViewUrls,
        Domain.ManageUrlBatches,
        Domain.ViewReports,
        Domain.ManageServiceAccounts,
        Domain.ViewServiceAccounts
    });

    /// <summary>
    /// Validates if a permission exists in the system
    /// </summary>
    public static bool IsValidPermission(string permission)
    {
        return AllPermissions.Contains(permission);
    }

    /// <summary>
    /// Gets permissions by scope
    /// </summary>
    public static IEnumerable<string> GetPermissionsByScope(string scope)
    {
        return scope.ToLowerInvariant() switch
        {
            "global" => AllPermissions.Where(p => p.StartsWith("global.")),
            "tenant" => AllPermissions.Where(p => p.StartsWith("tenant.")),
            "domain" => AllPermissions.Where(p => p.StartsWith("domain.")),
            _ => Enumerable.Empty<string>()
        };
    }
}
