using WebSearchIndexing.Modules.Identity.Domain.Entities;

namespace WebSearchIndexing.Modules.Identity.Domain.Repositories;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<User>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<User>> GetByTenantAsync(Guid tenantId, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<User>> GetByDomainAsync(Guid domainId, CancellationToken cancellationToken = default);
    Task<User> AddAsync(User user, CancellationToken cancellationToken = default);
    Task<User> UpdateAsync(User user, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<string>> GetUserGlobalRolesAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<(IEnumerable<User> Users, int TotalCount)> GetPagedAsync(
        int page, 
        int pageSize, 
        string? searchTerm = null, 
        CancellationToken cancellationToken = default);
}
