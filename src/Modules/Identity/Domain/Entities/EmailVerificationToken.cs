namespace WebSearchIndexing.Modules.Identity.Domain.Entities;

/// <summary>
/// Email verification token entity
/// </summary>
public sealed class EmailVerificationToken
{
    private EmailVerificationToken() { } // EF Core constructor

    public EmailVerificationToken(Guid userId, string email)
    {
        ArgumentNullException.ThrowIfNull(userId);
        ArgumentException.ThrowIfNullOrWhiteSpace(email);

        Id = Guid.NewGuid();
        UserId = userId;
        Email = email;
        Token = GenerateSecureToken();
        CreatedAt = DateTime.UtcNow;
        ExpiresAt = DateTime.UtcNow.AddHours(24);
        IsUsed = false;
    }

    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public string Email { get; private set; } = string.Empty;
    public string Token { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; }
    public DateTime ExpiresAt { get; private set; }
    public bool IsUsed { get; private set; }
    public DateTime? UsedAt { get; private set; }

    // Navigation properties
    public User? User { get; private set; }

    /// <summary>
    /// Checks if the token is valid and not expired
    /// </summary>
    public bool IsValid()
    {
        var now = DateTime.UtcNow;
        var isNotUsed = !IsUsed;
        var isNotExpired = now < ExpiresAt;
        
        return isNotUsed && isNotExpired;
    }

    /// <summary>
    /// Marks the token as used
    /// </summary>
    public void MarkAsUsed()
    {
        if (IsUsed)
            throw new InvalidOperationException("Token has already been used");

        if (DateTime.UtcNow >= ExpiresAt)
            throw new InvalidOperationException("Token has expired");

        IsUsed = true;
        UsedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Generates a secure random token
    /// </summary>
    private static string GenerateSecureToken()
    {
        var bytes = new byte[32];
        using var rng = System.Security.Cryptography.RandomNumberGenerator.Create();
        rng.GetBytes(bytes);
        return Convert.ToBase64String(bytes).Replace("+", "-").Replace("/", "_").TrimEnd('=');
    }
}
