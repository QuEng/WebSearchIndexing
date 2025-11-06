using MediatR;

namespace WebSearchIndexing.Modules.Identity.Application.Commands;

public record AssignRoleCommand(
    Guid UserId,
    Guid TenantId,
    string Role,
    Guid? AssignedBy = null) : IRequest;
