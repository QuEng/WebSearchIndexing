using MediatR;
using WebSearchIndexing.Modules.Identity.Application.DTOs;

namespace WebSearchIndexing.Modules.Identity.Application.Queries;

public record GetUserQuery(Guid UserId) : IRequest<UserDto?>;
