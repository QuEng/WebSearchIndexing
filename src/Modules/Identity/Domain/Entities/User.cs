using WebSearchIndexing.Modules.Identity.Domain.Common;
using WebSearchIndexing.Modules.Identity.Domain.Events;

namespace WebSearchIndexing.Modules.Identity.Domain.Entities;

public class User : AggregateRoot<Guid>
{
    public string Email { get; private set; } = default!;
    public string PasswordHash { get; private set; } = default!;
    public string FirstName { get; private set; } = default!;
    public string LastName { get; private set; } = default!;
    public DateTime CreatedAt { get; private set; }
    public bool IsActive { get; private set; }
    public bool IsEmailVerified { get; private set; }
    public DateTime? LastLoginAt { get; private set; }
    public DateTime? LastPasswordChangeAt { get; private set; }

    // Navigation properties
    private readonly List<UserTenant> _userTenants = [];
    public IReadOnlyCollection<UserTenant> UserTenants => _userTenants.AsReadOnly();

    private readonly List<UserDomain> _userDomains = [];
    public IReadOnlyCollection<UserDomain> UserDomains => _userDomains.AsReadOnly();

    private readonly List<PasswordHistory> _passwordHistory = [];
    public IReadOnlyCollection<PasswordHistory> PasswordHistory => _passwordHistory.AsReadOnly();

    // Private constructor for EF Core
    private User() { }

    public User(
        string email,
        string passwordHash,
        string firstName,
        string lastName)
    {
        Id = Guid.NewGuid();
        Email = email.ToLowerInvariant();
        PasswordHash = passwordHash;
        FirstName = firstName;
        LastName = lastName;
        CreatedAt = DateTime.UtcNow;
        IsActive = true;
        IsEmailVerified = false;

        // Publish domain event
        AddDomainEvent(new UserCreatedDomainEvent(Id, Email, FirstName, LastName, CreatedAt));
    }

    public void UpdateProfile(string firstName, string lastName)
    {
        FirstName = firstName;
        LastName = lastName;
    }

    public void ChangePassword(string newPasswordHash)
    {
        PasswordHash = newPasswordHash;
        LastPasswordChangeAt = DateTime.UtcNow;
    }

    public void VerifyEmail()
    {
        IsEmailVerified = true;
    }

    public void MarkEmailAsVerified()
    {
        if (IsEmailVerified)
        {
            throw new InvalidOperationException("Email is already verified");
        }
        
        IsEmailVerified = true;
        AddDomainEvent(new UserEmailVerifiedDomainEvent(Id, Email, DateTime.UtcNow));
    }

    public void Activate()
    {
        IsActive = true;
    }

    public void Deactivate()
    {
        IsActive = false;
    }

    public void RecordLogin()
    {
        LastLoginAt = DateTime.UtcNow;
    }

    public void AddTenantRole(Guid tenantId, string role)
    {
        var existingUserTenant = _userTenants.FirstOrDefault(ut => ut.TenantId == tenantId);
        if (existingUserTenant != null)
        {
            existingUserTenant.UpdateRole(role);
        }
        else
        {
            _userTenants.Add(new UserTenant(Id, tenantId, role));
        }
    }

    public void RemoveTenantRole(Guid tenantId)
    {
        var userTenant = _userTenants.FirstOrDefault(ut => ut.TenantId == tenantId);
        if (userTenant != null)
        {
            userTenant.Deactivate();
        }
    }

    public void AddDomainRole(Guid domainId, string role)
    {
        var existingUserDomain = _userDomains.FirstOrDefault(ud => ud.DomainId == domainId);
        if (existingUserDomain != null)
        {
            existingUserDomain.UpdateRole(role);
        }
        else
        {
            _userDomains.Add(new UserDomain(Id, domainId, role));
        }
    }

    public void RemoveDomainRole(Guid domainId)
    {
        var userDomain = _userDomains.FirstOrDefault(ud => ud.DomainId == domainId);
        if (userDomain != null)
        {
            userDomain.Deactivate();
        }
    }

    public string GetFullName() => $"{FirstName} {LastName}";
}
