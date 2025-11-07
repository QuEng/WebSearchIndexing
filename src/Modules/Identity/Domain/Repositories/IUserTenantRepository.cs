using WebSearchIndexing.Modules.Identity.Domain.Entities;

namespace WebSearchIndexing.Modules.Identity.Domain.Repositories;

public interface IUserTenantRepository
{
    Task<UserTenant?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<UserTenant?> GetByUserAndTenantAsync(Guid userId, Guid tenantId, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<UserTenant>> GetByUserAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<UserTenant>> GetByTenantAsync(Guid tenantId, CancellationToken cancellationToken = default);
    Task<UserTenant> AddAsync(UserTenant userTenant, CancellationToken cancellationToken = default);
    Task<UserTenant> UpdateAsync(UserTenant userTenant, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid userId, Guid tenantId, CancellationToken cancellationToken = default);
}
