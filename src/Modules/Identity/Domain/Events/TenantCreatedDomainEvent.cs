using WebSearchIndexing.Modules.Identity.Domain.Common;

namespace WebSearchIndexing.Modules.Identity.Domain.Events;

public record TenantCreatedDomainEvent(
    Guid TenantId,
    string Name,
    string Slug,
    Guid OwnerId,
    string Plan,
    DateTime CreatedAt) : IDomainEvent;
