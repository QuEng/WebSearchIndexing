namespace WebSearchIndexing.Modules.Identity.Application.UserManagement.Commands.InviteUser;

public sealed record InviteUserCommand(
    string Email,
    string Role,
    Guid? TenantId = null,
    Guid? DomainId = null,
    string? PersonalMessage = null
);
