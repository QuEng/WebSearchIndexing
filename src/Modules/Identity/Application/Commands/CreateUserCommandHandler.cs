using MediatR;
using Microsoft.Extensions.Logging;
using WebSearchIndexing.BuildingBlocks.Messaging;
using WebSearchIndexing.Modules.Identity.Application.Abstractions;
using WebSearchIndexing.Modules.Identity.Application.Authorization.IntegrationEvents;
using WebSearchIndexing.Modules.Identity.Domain.Repositories;
using WebSearchIndexing.Modules.Identity.Domain.Entities;
using WebSearchIndexing.Modules.Identity.Domain.ValueObjects;

namespace WebSearchIndexing.Modules.Identity.Application.Commands;

public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, Guid>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IIntegrationEventPublisher _eventPublisher;
    private readonly ILogger<CreateUserCommandHandler> _logger;

    public CreateUserCommandHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IIntegrationEventPublisher eventPublisher,
        ILogger<CreateUserCommandHandler> logger)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));
        _eventPublisher = eventPublisher ?? throw new ArgumentNullException(nameof(eventPublisher));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
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

        // Publish integration event
        await _eventPublisher.PublishAsync(
            new UserRegisteredEvent(
                Guid.Empty.ToString(), // Default tenant for now
                user.Id,
                user.Email,
                user.FirstName,
                user.LastName,
                DateTime.UtcNow),
            cancellationToken);

        _logger.LogInformation(
            "User {UserId} ({Email}) was created",
            user.Id, user.Email);

        return user.Id;
    }
}
