namespace WebSearchIndexing.Modules.Identity.Domain.Entities;

public class RefreshToken
{
    public Guid Id { get; private set; }
    public string Token { get; private set; } = string.Empty;
    public Guid UserId { get; private set; }
    public Guid? TenantId { get; private set; }
    public DateTime ExpiresAt { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public bool IsRevoked { get; private set; }
    public string? RevokedReason { get; private set; }
    public DateTime? RevokedAt { get; private set; }
    public string CreatedByIp { get; private set; } = string.Empty;

    // Navigation properties
    public User User { get; private set; } = null!;

    private RefreshToken() { } // For EF Core

    public RefreshToken(
        string token,
        Guid userId,
        DateTime expiresAt,
        string createdByIp,
        Guid? tenantId = null)
    {
        Id = Guid.NewGuid();
        Token = token ?? throw new ArgumentNullException(nameof(token));
        UserId = userId;
        TenantId = tenantId;
        ExpiresAt = expiresAt;
        CreatedAt = DateTime.UtcNow;
        CreatedByIp = createdByIp ?? throw new ArgumentNullException(nameof(createdByIp));
        IsRevoked = false;
    }

    public void Revoke(string reason)
    {
        if (IsRevoked)
            return;

        IsRevoked = true;
        RevokedReason = reason;
        RevokedAt = DateTime.UtcNow;
    }

    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;

    public bool IsValid => !IsRevoked && !IsExpired;
}
