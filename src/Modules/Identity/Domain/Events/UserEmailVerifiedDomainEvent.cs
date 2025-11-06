using WebSearchIndexing.Modules.Identity.Domain.Common;

namespace WebSearchIndexing.Modules.Identity.Domain.Events;

public sealed record UserEmailVerifiedDomainEvent(
    Guid UserId,
    string Email,
    DateTime VerifiedAt) : IDomainEvent;
