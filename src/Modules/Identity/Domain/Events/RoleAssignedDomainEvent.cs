using WebSearchIndexing.Modules.Identity.Domain.Common;

namespace WebSearchIndexing.Modules.Identity.Domain.Events;

public record RoleAssignedDomainEvent(
    Guid UserId,
    Guid TenantId,
    string Role,
    Guid? AssignedBy,
    DateTime AssignedAt) : IDomainEvent;
