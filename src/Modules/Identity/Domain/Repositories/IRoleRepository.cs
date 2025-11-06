using WebSearchIndexing.Modules.Identity.Domain.Entities;

namespace WebSearchIndexing.Modules.Identity.Domain.Repositories;

public interface IRoleRepository
{
    Task<Role?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Role?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<Role>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<Role>> GetByTypeAsync(RoleType type, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<Role>> GetGlobalRolesAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<Role>> GetTenantRolesAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<Role>> GetDomainRolesAsync(CancellationToken cancellationToken = default);
    Task<Role> AddAsync(Role role, CancellationToken cancellationToken = default);
    Task<Role> UpdateAsync(Role role, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default);
}
