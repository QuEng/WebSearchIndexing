using WebSearchIndexing.Modules.Identity.Domain.Common;
using WebSearchIndexing.Modules.Identity.Domain.Entities;

namespace WebSearchIndexing.Modules.Identity.Domain.Entities;

public class UserInvitation : Entity<Guid>
{
    public string Email { get; private set; } = default!;
    public Guid InvitedByUserId { get; private set; }
    public Guid? TenantId { get; private set; }
    public Guid? DomainId { get; private set; }
    public string Role { get; private set; } = default!;
    public string InvitationToken { get; private set; } = default!;
    public DateTime CreatedAt { get; private set; }
    public DateTime ExpiresAt { get; private set; }
    public bool IsUsed { get; private set; }
    public DateTime? UsedAt { get; private set; }
    public Guid? AcceptedByUserId { get; private set; }
    public bool IsRevoked { get; private set; }
    public DateTime? RevokedAt { get; private set; }

    // Private constructor for EF Core
    private UserInvitation() { }

    public UserInvitation(
        string email,
        Guid invitedByUserId,
        string role,
        Guid? tenantId = null,
        Guid? domainId = null,
        TimeSpan? validFor = null)
    {
        Id = Guid.NewGuid();
        Email = email.ToLowerInvariant();
        InvitedByUserId = invitedByUserId;
        TenantId = tenantId;
        DomainId = domainId;
        Role = role;
        InvitationToken = GenerateToken();
        CreatedAt = DateTime.UtcNow;
        ExpiresAt = DateTime.UtcNow.Add(validFor ?? TimeSpan.FromDays(7));
        IsUsed = false;
        IsRevoked = false;
    }

    public void Accept(Guid userId)
    {
        if (IsUsed)
            throw new InvalidOperationException("Invitation has already been used");

        if (IsRevoked)
            throw new InvalidOperationException("Invitation has been revoked");

        if (DateTime.UtcNow > ExpiresAt)
            throw new InvalidOperationException("Invitation has expired");

        IsUsed = true;
        UsedAt = DateTime.UtcNow;
        AcceptedByUserId = userId;
    }

    public void Revoke()
    {
        if (IsUsed)
            throw new InvalidOperationException("Cannot revoke a used invitation");

        IsRevoked = true;
        RevokedAt = DateTime.UtcNow;
    }

    public bool IsValid()
    {
        return !IsUsed && !IsRevoked && DateTime.UtcNow <= ExpiresAt;
    }

    private static string GenerateToken()
    {
        return Convert.ToBase64String(Guid.NewGuid().ToByteArray())
            .Replace("/", "_")
            .Replace("+", "-")
            .Replace("=", "");
    }
}
