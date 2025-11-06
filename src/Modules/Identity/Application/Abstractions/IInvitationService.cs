using WebSearchIndexing.Modules.Identity.Application.DTOs;

namespace WebSearchIndexing.Modules.Identity.Application.Abstractions;

public interface IInvitationService
{
    Task<InvitationResult> GetIncomingInvitationsAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<InvitationResult> GetOutgoingInvitationsAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<InvitationDetailsResult> GetInvitationByTokenAsync(string token, CancellationToken cancellationToken = default);
    Task<OperationResult> AcceptInvitationAsync(Guid invitationId, Guid userId, CancellationToken cancellationToken = default);
    Task<OperationResult> DeclineInvitationAsync(Guid invitationId, Guid userId, CancellationToken cancellationToken = default);
    Task<OperationResult> RevokeInvitationAsync(Guid invitationId, Guid userId, CancellationToken cancellationToken = default);
}
