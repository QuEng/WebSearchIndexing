using WebSearchIndexing.Modules.Catalog.Domain.Entities;

namespace WebSearchIndexing.Modules.Catalog.Application.Abstractions;

public interface IServiceAccountRepository
{
    Task<ServiceAccount> AddAsync(ServiceAccount entity, CancellationToken cancellationToken = default);

    Task<bool> UpdateAsync(ServiceAccount entity, CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);

    Task<ServiceAccount?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<List<ServiceAccount>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<bool> EntityExistsAsync(Guid id, CancellationToken cancellationToken = default);

    Task<bool> EntityExistByProjectIdAsync(string projectId, CancellationToken cancellationToken = default);

    Task<int> GetCountAsync(CancellationToken cancellationToken = default);

    Task<int> GetQuotaByAllAsync(CancellationToken cancellationToken = default);

    Task<int> GetQuotaAvailableTodayAsync(CancellationToken cancellationToken = default);

    Task<ServiceAccount?> GetWithAvailableLimitAsync(CancellationToken cancellationToken = default);
}
