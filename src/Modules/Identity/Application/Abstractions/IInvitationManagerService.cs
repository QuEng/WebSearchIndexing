using WebSearchIndexing.Modules.Identity.Application.DTOs;

namespace WebSearchIndexing.Modules.Identity.Application.Abstractions;

public interface IInvitationManagerService
{
    Task<OperationResult> SendInvitationAsync(
        string email, 
        Guid invitedByUserId, 
        string role, 
        Guid? tenantId = null, 
        Guid? domainId = null, 
        CancellationToken cancellationToken = default);

    Task<OperationResult> ResendInvitationAsync(
        Guid invitationId, 
        Guid userId, 
        CancellationToken cancellationToken = default);

    Task<InvitationResult> GetInvitationsByTenantAsync(
        Guid tenantId, 
        CancellationToken cancellationToken = default);

    Task<OperationResult> BulkInviteAsync(
        IEnumerable<string> emails, 
        Guid invitedByUserId, 
        string role, 
        Guid? tenantId = null, 
        Guid? domainId = null, 
        CancellationToken cancellationToken = default);
}
