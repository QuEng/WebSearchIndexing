using WebSearchIndexing.Modules.Identity.Domain.Common;
using WebSearchIndexing.Modules.Identity.Domain.Constants;

namespace WebSearchIndexing.Modules.Identity.Domain.Entities;

public class UserTenant : Entity<Guid>
{
    public Guid UserId { get; private set; }
    public Guid TenantId { get; private set; }
    public string Role { get; private set; } = default!;
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? DeactivatedAt { get; private set; }
    public Guid? InvitedBy { get; private set; }

    // Navigation properties
    public User User { get; private set; } = default!;
    public Tenant Tenant { get; private set; } = default!;

    // Private constructor for EF Core
    private UserTenant() { }

    public UserTenant(
        Guid userId,
        Guid tenantId,
        string role,
        Guid? invitedBy = null)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        TenantId = tenantId;
        Role = role;
        IsActive = true;
        CreatedAt = DateTime.UtcNow;
        InvitedBy = invitedBy;
    }

    public void UpdateRole(string newRole)
    {
        Role = newRole;
    }

    public void Activate()
    {
        IsActive = true;
        DeactivatedAt = null;
    }

    public void Deactivate()
    {
        IsActive = false;
        DeactivatedAt = DateTime.UtcNow;
    }

    public bool IsOwner() => Role.Equals(Roles.Owner, StringComparison.OrdinalIgnoreCase);
    public bool IsAdmin() => Role.Equals(Roles.Admin, StringComparison.OrdinalIgnoreCase);
    public bool IsMember() => Role.Equals(Roles.Member, StringComparison.OrdinalIgnoreCase);
    public bool IsViewer() => Role.Equals(Roles.Viewer, StringComparison.OrdinalIgnoreCase);

    public bool CanManageUsers() => Roles.Hierarchy.CanManageUsers(Role);
    public bool CanManageDomains() => Roles.Hierarchy.CanManageDomains(Role);
    public bool CanViewData() => IsActive;
}
