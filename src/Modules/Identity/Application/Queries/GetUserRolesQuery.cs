using MediatR;
using WebSearchIndexing.Modules.Identity.Application.DTOs;

namespace WebSearchIndexing.Modules.Identity.Application.Queries;

public record GetUserRolesQuery(Guid UserId) : IRequest<IEnumerable<UserRoleDto>>;
