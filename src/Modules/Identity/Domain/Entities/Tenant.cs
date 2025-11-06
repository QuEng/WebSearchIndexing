using WebSearchIndexing.Modules.Identity.Domain.Common;
using WebSearchIndexing.Modules.Identity.Domain.Events;

namespace WebSearchIndexing.Modules.Identity.Domain.Entities;

public class Tenant : AggregateRoot<Guid>
{
    public string Name { get; private set; } = default!;
    public string Slug { get; private set; } = default!;
    public string Plan { get; private set; } = default!;
    public DateTime CreatedAt { get; private set; }
    public bool IsActive { get; private set; }
    public Guid OwnerId { get; private set; }

    // Settings
    public int MaxUsers { get; private set; }
    public int MaxDomains { get; private set; }
    public int DailyUrlQuota { get; private set; }
    public int DailyInspectionQuota { get; private set; }

    // Navigation properties
    private readonly List<UserTenant> _userTenants = [];
    public IReadOnlyCollection<UserTenant> UserTenants => _userTenants.AsReadOnly();

    // Private constructor for EF Core
    private Tenant() { }

    public Tenant(
        string name,
        string slug,
        Guid ownerId,
        string plan = "Free")
    {
        Id = Guid.NewGuid();
        Name = name;
        Slug = slug.ToLowerInvariant();
        OwnerId = ownerId;
        Plan = plan;
        CreatedAt = DateTime.UtcNow;
        IsActive = true;

        // Default quotas based on plan
        SetPlanQuotas(plan);

        // Publish domain event
        AddDomainEvent(new TenantCreatedDomainEvent(Id, Name, Slug, OwnerId, Plan, CreatedAt));
    }

    public void UpdateDetails(string name, string slug)
    {
        Name = name;
        Slug = slug.ToLowerInvariant();
    }

    public void ChangePlan(string newPlan)
    {
        Plan = newPlan;
        SetPlanQuotas(newPlan);
    }

    public void TransferOwnership(Guid newOwnerId)
    {
        OwnerId = newOwnerId;
    }

    public void Activate()
    {
        IsActive = true;
    }

    public void Deactivate()
    {
        IsActive = false;
    }

    public void AddUser(Guid userId, string role)
    {
        if (_userTenants.Count >= MaxUsers)
        {
            throw new InvalidOperationException($"Cannot add user. Maximum users ({MaxUsers}) reached for plan {Plan}");
        }

        var existingUserTenant = _userTenants.FirstOrDefault(ut => ut.UserId == userId);
        if (existingUserTenant != null)
        {
            existingUserTenant.UpdateRole(role);
        }
        else
        {
            _userTenants.Add(new UserTenant(userId, Id, role));
        }
    }

    public void RemoveUser(Guid userId)
    {
        var userTenant = _userTenants.FirstOrDefault(ut => ut.UserId == userId);
        if (userTenant != null)
        {
            userTenant.Deactivate();
        }
    }

    public bool IsOwner(Guid userId) => OwnerId == userId;

    public bool CanAddDomain() => true; // Would check against MaxDomains when domains are implemented

    private void SetPlanQuotas(string plan)
    {
        switch (plan.ToLowerInvariant())
        {
            case "free":
                MaxUsers = 3;
                MaxDomains = 1;
                DailyUrlQuota = 100;
                DailyInspectionQuota = 50;
                break;
            case "basic":
                MaxUsers = 10;
                MaxDomains = 5;
                DailyUrlQuota = 1000;
                DailyInspectionQuota = 500;
                break;
            case "pro":
                MaxUsers = 50;
                MaxDomains = 20;
                DailyUrlQuota = 10000;
                DailyInspectionQuota = 5000;
                break;
            case "enterprise":
                MaxUsers = int.MaxValue;
                MaxDomains = int.MaxValue;
                DailyUrlQuota = int.MaxValue;
                DailyInspectionQuota = int.MaxValue;
                break;
            default:
                goto case "free";
        }
    }
}
