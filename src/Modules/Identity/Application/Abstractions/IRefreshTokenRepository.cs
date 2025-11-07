using WebSearchIndexing.Modules.Identity.Domain.Entities;

namespace WebSearchIndexing.Modules.Identity.Application.Abstractions;

public interface IRefreshTokenRepository
{
    Task<RefreshToken?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<RefreshToken>> GetActiveByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<RefreshToken>> GetActiveSessions(CancellationToken cancellationToken = default);
    Task<RefreshToken> AddAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default);
    Task UpdateAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default);
    Task DeleteAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default);
    Task RevokeAllUserTokensAsync(Guid userId, string reason, CancellationToken cancellationToken = default);
    Task DeleteExpiredAsync(CancellationToken cancellationToken = default);
}
