using WebSearchIndexing.Modules.Identity.Domain.Entities;

namespace WebSearchIndexing.Modules.Identity.Domain.Repositories;

public interface ITenantRepository
{
    Task<Tenant?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Tenant?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<Tenant>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Tenant>> GetByOwnerAsync(Guid ownerId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Tenant>> GetByUserAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<Tenant> AddAsync(Tenant tenant, CancellationToken cancellationToken = default);
    Task<Tenant> UpdateAsync(Tenant tenant, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> ExistsBySlugAsync(string slug, CancellationToken cancellationToken = default);
    Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default);
    Task<(IEnumerable<Tenant> Tenants, int TotalCount)> GetPagedAsync(
        int page, 
        int pageSize, 
        string? searchTerm = null, 
        CancellationToken cancellationToken = default);
}
