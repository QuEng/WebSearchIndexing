using MediatR;

namespace WebSearchIndexing.Modules.Identity.Application.Commands;

public record CreateTenantCommand(
    string Name,
    string Slug,
    Guid OwnerId,
    string Plan = "Free") : IRequest<Guid>;
