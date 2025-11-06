namespace WebSearchIndexing.Modules.Identity.Application.Authorization.Commands.AssignRole;

public sealed record AssignRoleCommand(
    Guid UserId,
    string Role,
    Guid? TenantId = null,
    Guid? DomainId = null
);
