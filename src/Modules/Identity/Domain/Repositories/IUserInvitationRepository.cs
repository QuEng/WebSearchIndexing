using WebSearchIndexing.Modules.Identity.Domain.Entities;

namespace WebSearchIndexing.Modules.Identity.Domain.Repositories;

public interface IUserInvitationRepository
{
    Task<UserInvitation?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<UserInvitation?> GetByTokenAsync(string token, CancellationToken cancellationToken = default);
    Task<UserInvitation?> GetPendingByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<UserInvitation>> GetByTenantAsync(Guid tenantId, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<UserInvitation>> GetByDomainAsync(Guid domainId, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<UserInvitation>> GetPendingInvitationsAsync(CancellationToken cancellationToken = default);
    Task<UserInvitation> AddAsync(UserInvitation invitation, CancellationToken cancellationToken = default);
    Task<UserInvitation> UpdateAsync(UserInvitation invitation, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
