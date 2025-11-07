using WebSearchIndexing.Modules.Identity.Domain.Entities;

namespace WebSearchIndexing.Modules.Identity.Domain.Repositories;

public interface ILoginHistoryRepository
{
    Task<LoginHistory> AddAsync(LoginHistory loginHistory, CancellationToken cancellationToken = default);
    Task<LoginHistory?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<LoginHistory>> GetByUserIdAsync(
        Guid userId, 
        int limit = 50, 
        CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<LoginHistory>> GetRecentByUserIdAsync(
        Guid userId, 
        int days = 30,
        int limit = 50, 
        CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<LoginHistory>> GetFailedAttemptsAsync(
        string ipAddress, 
        TimeSpan timeWindow,
        CancellationToken cancellationToken = default);
    Task DeleteOldRecordsAsync(DateTime beforeDate, CancellationToken cancellationToken = default);
}
