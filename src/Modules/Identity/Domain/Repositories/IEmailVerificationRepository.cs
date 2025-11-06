using WebSearchIndexing.Modules.Identity.Domain.Entities;

namespace WebSearchIndexing.Modules.Identity.Domain.Repositories;

/// <summary>
/// Repository for managing email verification tokens
/// </summary>
public interface IEmailVerificationRepository
{
    /// <summary>
    /// Creates a new email verification token
    /// </summary>
    Task<EmailVerificationToken> CreateTokenAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a verification token by its value
    /// </summary>
    Task<EmailVerificationToken?> GetByTokenAsync(string token, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all verification tokens for a user
    /// </summary>
    Task<IReadOnlyCollection<EmailVerificationToken>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks a token as used
    /// </summary>
    Task MarkAsUsedAsync(Guid tokenId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes expired tokens
    /// </summary>
    Task DeleteExpiredTokensAsync(CancellationToken cancellationToken = default);
}
