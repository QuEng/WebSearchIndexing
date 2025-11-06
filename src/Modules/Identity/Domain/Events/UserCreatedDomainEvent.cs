using WebSearchIndexing.Modules.Identity.Domain.Common;

namespace WebSearchIndexing.Modules.Identity.Domain.Events;

public record UserCreatedDomainEvent(
    Guid UserId,
    string Email,
    string FirstName,
    string LastName,
    DateTime CreatedAt) : IDomainEvent;
