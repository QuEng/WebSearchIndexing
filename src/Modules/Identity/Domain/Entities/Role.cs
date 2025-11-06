using WebSearchIndexing.Modules.Identity.Domain.Common;

namespace WebSearchIndexing.Modules.Identity.Domain.Entities;

public class Role : Entity<Guid>
{
    public string Name { get; private set; } = default!;
    public RoleType Type { get; private set; }
    public string Description { get; private set; } = default!;
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }

    // Permissions as JSON or comma-separated string
    public string Permissions { get; private set; } = default!;

    // Private constructor for EF Core
    private Role() { }

    public Role(
        string name,
        RoleType type,
        string description,
        string permissions = "")
    {
        Id = Guid.NewGuid();
        Name = name;
        Type = type;
        Description = description;
        Permissions = permissions;
        IsActive = true;
        CreatedAt = DateTime.UtcNow;
    }

    // Constructor for seed data with explicit values
    public Role(
        Guid id,
        string name,
        RoleType type,
        string permissions,
        DateTime createdAt,
        bool isActive)
    {
        Id = id;
        Name = name;
        Type = type;
        Description = string.Empty; // Use empty for seed data
        Permissions = permissions;
        IsActive = isActive;
        CreatedAt = createdAt;
    }

    public void UpdateDetails(string name, string description)
    {
        Name = name;
        Description = description;
    }

    public void UpdatePermissions(string permissions)
    {
        Permissions = permissions;
    }

    public void Activate()
    {
        IsActive = true;
    }

    public void Deactivate()
    {
        IsActive = false;
    }

    public bool HasPermission(string permission)
    {
        if (string.IsNullOrEmpty(Permissions))
            return false;

        return Permissions.Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Any(p => p.Trim().Equals(permission, StringComparison.OrdinalIgnoreCase));
    }
}

public enum RoleType
{
    Global = 1,     // System-wide roles (GlobalAdmin)
    Tenant = 2,     // Tenant-specific roles (Owner, Admin, Member, Viewer)
    Domain = 3      // Domain-specific roles (DomainAdmin, DomainEditor, DomainViewer)
}
