using MediatR;

namespace WebSearchIndexing.Modules.Identity.Application.Commands;

public record CreateUserCommand(
    string Email,
    string Password,
    string FirstName,
    string LastName) : IRequest<Guid>;
