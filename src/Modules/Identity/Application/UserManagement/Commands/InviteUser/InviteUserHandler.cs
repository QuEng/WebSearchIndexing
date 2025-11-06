using Microsoft.Extensions.Logging;
using WebSearchIndexing.BuildingBlocks.Messaging;
using WebSearchIndexing.Modules.Identity.Application.Authorization.IntegrationEvents;
using WebSearchIndexing.Modules.Identity.Application.Services;
using WebSearchIndexing.Modules.Identity.Application.UserManagement.DTOs;
using WebSearchIndexing.Modules.Identity.Domain.Entities;
using WebSearchIndexing.Modules.Identity.Domain.Repositories;

namespace WebSearchIndexing.Modules.Identity.Application.UserManagement.Commands.InviteUser;

public sealed class InviteUserHandler
{
    private readonly IUserRepository _userRepository;
    private readonly IUserInvitationRepository _invitationRepository;
    private readonly IIdentityEmailService _emailService;
    private readonly IIntegrationEventPublisher _eventPublisher;
    private readonly ILogger<InviteUserHandler> _logger;

    public InviteUserHandler(
        IUserRepository userRepository,
        IUserInvitationRepository invitationRepository,
        IIdentityEmailService emailService,
        IIntegrationEventPublisher eventPublisher,
        ILogger<InviteUserHandler> logger)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _invitationRepository = invitationRepository ?? throw new ArgumentNullException(nameof(invitationRepository));
        _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
        _eventPublisher = eventPublisher ?? throw new ArgumentNullException(nameof(eventPublisher));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<UserInvitationDto> HandleAsync(InviteUserCommand command, Guid invitedByUserId, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        // Check if user already exists
        var existingUser = await _userRepository.GetByEmailAsync(command.Email, cancellationToken);
        if (existingUser != null)
        {
            throw new InvalidOperationException($"User with email '{command.Email}' already exists");
        }

        // Check if there's already a pending invitation
        var existingInvitation = await _invitationRepository.GetPendingByEmailAsync(command.Email, cancellationToken);
        if (existingInvitation != null)
        {
            throw new InvalidOperationException($"There's already a pending invitation for '{command.Email}'");
        }

        // Get inviter information
        var inviter = await _userRepository.GetByIdAsync(invitedByUserId, cancellationToken);
        if (inviter == null)
        {
            throw new InvalidOperationException("Inviter not found");
        }

        // Get organization name
        var organizationName = "WebSearchIndexing"; // Default organization name

        // Create invitation
        var invitation = new UserInvitation(
            command.Email,
            invitedByUserId,
            command.Role,
            command.TenantId,
            command.DomainId);

        var savedInvitation = await _invitationRepository.AddAsync(invitation, cancellationToken);

        // Generate invitation link
        var invitationLink = GenerateInvitationLink(savedInvitation.InvitationToken);

        // Send invitation email
        await _emailService.SendUserInvitationAsync(
            command.Email,
            command.Email, // Use email as name until user registers
            inviter.GetFullName(),
            organizationName,
            invitationLink,
            command.Role,
            cancellationToken);

        // Publish integration event
        var tenantId = command.TenantId?.ToString() ?? command.DomainId?.ToString() ?? Guid.Empty.ToString();
        await _eventPublisher.PublishAsync(
            new UserInvitedEvent(
                tenantId,
                savedInvitation.Id,
                command.Email,
                command.Role,
                invitedByUserId,
                organizationName,
                DateTime.UtcNow),
            cancellationToken);

        _logger.LogInformation(
            "User invitation sent to {Email} by {InviterName} for role {Role}",
            command.Email, inviter.GetFullName(), command.Role);

        return UserInvitationDto.FromDomain(savedInvitation);
    }

    private static string GenerateInvitationLink(string token)
    {
        // In a real application, this would be configured
        var baseUrl = "https://your-app.com"; // Should come from configuration
        return $"{baseUrl}/invitation/accept?token={token}";
    }
}
