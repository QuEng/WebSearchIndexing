using WebSearchIndexing.Modules.Identity.Domain.Common;

namespace WebSearchIndexing.Modules.Identity.Domain.Events;

public record DomainRoleAssignedDomainEvent(
    Guid UserId,
    Guid DomainId,
    string Role,
    Guid? GrantedBy,
    DateTime GrantedAt) : IDomainEvent;
