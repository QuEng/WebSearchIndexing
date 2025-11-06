namespace WebSearchIndexing.Modules.Identity.Domain.Entities;

/// <summary>
/// Stores password history for enforcing password reuse policies
/// </summary>
public sealed class PasswordHistory
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public string PasswordHash { get; private set; } = default!;
    public DateTime CreatedAt { get; private set; }

    // Navigation property
    public User User { get; private set; } = default!;

    // Private constructor for EF Core
    private PasswordHistory() { }

    public PasswordHistory(Guid userId, string passwordHash)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(passwordHash);
        
        Id = Guid.NewGuid();
        UserId = userId;
        PasswordHash = passwordHash;
        CreatedAt = DateTime.UtcNow;
    }
}
