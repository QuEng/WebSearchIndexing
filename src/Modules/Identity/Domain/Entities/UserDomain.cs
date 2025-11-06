using WebSearchIndexing.Modules.Identity.Domain.Common;
using WebSearchIndexing.Modules.Identity.Domain.Constants;

namespace WebSearchIndexing.Modules.Identity.Domain.Entities;

public class UserDomain : Entity<Guid>
{
    public Guid UserId { get; private set; }
    public Guid DomainId { get; private set; }
    public string Role { get; private set; } = default!;
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? DeactivatedAt { get; private set; }
    public Guid? GrantedBy { get; private set; }

    // Navigation properties
    public User User { get; private set; } = default!;

    // Private constructor for EF Core
    private UserDomain() { }

    public UserDomain(
        Guid userId,
        Guid domainId,
        string role,
        Guid? grantedBy = null)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        DomainId = domainId;
        Role = role;
        IsActive = true;
        CreatedAt = DateTime.UtcNow;
        GrantedBy = grantedBy;
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

    public bool IsDomainAdmin() => Role.Equals(Roles.DomainAdmin, StringComparison.OrdinalIgnoreCase);
    public bool IsDomainEditor() => Role.Equals(Roles.DomainEditor, StringComparison.OrdinalIgnoreCase);
    public bool IsDomainViewer() => Role.Equals(Roles.DomainViewer, StringComparison.OrdinalIgnoreCase);

    public bool CanManageDomain() => Roles.Hierarchy.IsDomainAdmin(Role);
    public bool CanEditUrls() => IsDomainAdmin() || IsDomainEditor();
    public bool CanViewStats() => IsActive;
}
