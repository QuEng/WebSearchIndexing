using Microsoft.Extensions.Logging;
using WebSearchIndexing.BuildingBlocks.Messaging;
using WebSearchIndexing.Modules.Identity.Application.Authorization.IntegrationEvents;
using WebSearchIndexing.Modules.Identity.Domain.Constants;
using WebSearchIndexing.Modules.Identity.Domain.Entities;
using WebSearchIndexing.Modules.Identity.Domain.Repositories;

namespace WebSearchIndexing.Modules.Identity.Application.Authorization.Commands.AssignRole;

public sealed class AssignRoleHandler
{
    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly IIntegrationEventPublisher _eventPublisher;
    private readonly ILogger<AssignRoleHandler> _logger;

    public AssignRoleHandler(
        IUserRepository userRepository,
        IRoleRepository roleRepository,
        IIntegrationEventPublisher eventPublisher,
        ILogger<AssignRoleHandler> logger)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _roleRepository = roleRepository ?? throw new ArgumentNullException(nameof(roleRepository));
        _eventPublisher = eventPublisher ?? throw new ArgumentNullException(nameof(eventPublisher));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task HandleAsync(AssignRoleCommand command, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        // Validate role exists
        var role = await _roleRepository.GetByNameAsync(command.Role, cancellationToken);
        if (role == null || !role.IsActive)
        {
            throw new InvalidOperationException($"Role '{command.Role}' not found or inactive");
        }

        // Get user
        var user = await _userRepository.GetByIdAsync(command.UserId, cancellationToken);
        if (user == null || !user.IsActive)
        {
            throw new InvalidOperationException($"User with ID '{command.UserId}' not found or inactive");
        }

        // Assign role based on type
        switch (role.Type)
        {
            case RoleType.Global:
                await AssignGlobalRoleAsync(user, command.Role, cancellationToken);
                break;

            case RoleType.Tenant:
                if (!command.TenantId.HasValue)
                    throw new ArgumentException("TenantId is required for tenant roles");
                user.AddTenantRole(command.TenantId.Value, command.Role);
                break;

            case RoleType.Domain:
                if (!command.DomainId.HasValue)
                    throw new ArgumentException("DomainId is required for domain roles");
                user.AddDomainRole(command.DomainId.Value, command.Role);
                break;

            default:
                throw new ArgumentException($"Unknown role type: {role.Type}");
        }

        // Save changes
        await _userRepository.UpdateAsync(user, cancellationToken);

        // Publish integration event
        var tenantId = GetTenantId(command);
        await _eventPublisher.PublishAsync(
            new RoleAssignedEvent(
                tenantId.ToString(),
                command.UserId,
                command.Role,
                role.Type,
                command.TenantId,
                command.DomainId),
            cancellationToken);

        _logger.LogInformation(
            "Assigned role {Role} of type {RoleType} to user {UserId}",
            command.Role, role.Type, command.UserId);
    }

    private async Task AssignGlobalRoleAsync(User user, string roleName, CancellationToken cancellationToken)
    {
        // Global roles are typically handled differently as they're not part of UserTenant/UserDomain
        // For now, we'll create a special tenant entry for global roles
        var globalTenantId = Guid.Empty; // Special ID for global roles
        user.AddTenantRole(globalTenantId, roleName);
    }

    private static string GetTenantId(AssignRoleCommand command)
    {
        return command.TenantId?.ToString() ?? command.DomainId?.ToString() ?? Guid.Empty.ToString();
    }
}
