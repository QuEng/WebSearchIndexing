using MediatR;
using WebSearchIndexing.Modules.Identity.Application.Abstractions;
using WebSearchIndexing.Modules.Identity.Domain.Repositories;
using WebSearchIndexing.Modules.Identity.Domain.Entities;
using WebSearchIndexing.Modules.Identity.Domain.ValueObjects;

namespace WebSearchIndexing.Modules.Identity.Application.Commands;

public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, Guid>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;

    public CreateUserCommandHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
    }

    public async Task<Guid> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        // Validate email
        var email = new Email(request.Email);
        
        // Check if user already exists
        if (await _userRepository.ExistsByEmailAsync(email.Value, cancellationToken))
        {
            throw new InvalidOperationException($"User with email '{email.Value}' already exists");
        }

        // Validate password
        var password = new Password(request.Password);
        var passwordHash = _passwordHasher.HashPassword(password.Value);

        // Create user
        var user = new User(
            email.Value,
            passwordHash,
            request.FirstName,
            request.LastName);

        await _userRepository.AddAsync(user, cancellationToken);

        return user.Id;
    }
}
