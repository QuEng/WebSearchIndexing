using Microsoft.AspNetCore.Authorization;

namespace WebSearchIndexing.Modules.Identity.Application.Authorization.Policies;

/// <summary>
/// Authorization requirements for different permission types
/// </summary>
public class GlobalPermissionRequirement : IAuthorizationRequirement
{
    public string Permission { get; }

    public GlobalPermissionRequirement(string permission)
    {
        Permission = permission ?? throw new ArgumentNullException(nameof(permission));
    }
}

public class TenantPermissionRequirement : IAuthorizationRequirement
{
    public string Permission { get; }

    public TenantPermissionRequirement(string permission)
    {
        Permission = permission ?? throw new ArgumentNullException(nameof(permission));
    }
}

public class DomainPermissionRequirement : IAuthorizationRequirement
{
    public string Permission { get; }

    public DomainPermissionRequirement(string permission)
    {
        Permission = permission ?? throw new ArgumentNullException(nameof(permission));
    }
}

public class ResourceAccessRequirement : IAuthorizationRequirement
{
    // This requirement will be evaluated dynamically based on resource context
}

public class TenantRoleRequirement : IAuthorizationRequirement
{
    public string Role { get; }

    public TenantRoleRequirement(string role)
    {
        Role = role ?? throw new ArgumentNullException(nameof(role));
    }
}

/// <summary>
/// Context for resource-based authorization
/// </summary>
public class ResourceAuthorizationContext
{
    public string ResourceType { get; }
    public Guid ResourceId { get; }
    public string Permission { get; }

    public ResourceAuthorizationContext(string resourceType, Guid resourceId, string permission)
    {
        ResourceType = resourceType ?? throw new ArgumentNullException(nameof(resourceType));
        ResourceId = resourceId;
        Permission = permission ?? throw new ArgumentNullException(nameof(permission));
    }
}
