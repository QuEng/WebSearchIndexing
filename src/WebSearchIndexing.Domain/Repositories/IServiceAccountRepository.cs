using WebSearchIndexing.Modules.Catalog.Domain;

namespace WebSearchIndexing.Domain.Repositories;

public interface IServiceAccountRepository : IRepository<ServiceAccount, Guid>
{
    Task<bool> EntityExistByProjectIdAsync(string projectId);
    Task<ServiceAccount?> GetWithAvailableLimitAsync();
    Task<int> GetCountAsync();
    Task<int> GetQuotaByAllAsync();
    Task<int> GetQuotaAvailableTodayAsync();
}
