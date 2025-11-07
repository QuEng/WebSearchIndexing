using MediatR;
using Microsoft.Extensions.Logging;
using WebSearchIndexing.BuildingBlocks.Messaging;
using WebSearchIndexing.Modules.Identity.Application.Authorization.IntegrationEvents;
using WebSearchIndexing.Modules.Identity.Domain.Repositories;
using WebSearchIndexing.Modules.Identity.Domain.Entities;

namespace WebSearchIndexing.Modules.Identity.Application.Commands;

public class CreateTenantCommandHandler : IRequestHandler<CreateTenantCommand, Guid>
{
    private readonly ITenantRepository _tenantRepository;
    private readonly IUserRepository _userRepository;
    private readonly IIntegrationEventPublisher _eventPublisher;
    private readonly ILogger<CreateTenantCommandHandler> _logger;

    public CreateTenantCommandHandler(
        ITenantRepository tenantRepository,
        IUserRepository userRepository,
        IIntegrationEventPublisher eventPublisher,
        ILogger<CreateTenantCommandHandler> logger)
    {
        _tenantRepository = tenantRepository ?? throw new ArgumentNullException(nameof(tenantRepository));
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _eventPublisher = eventPublisher ?? throw new ArgumentNullException(nameof(eventPublisher));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Guid> Handle(CreateTenantCommand request, CancellationToken cancellationToken)
    {
        // Validate owner exists
        var owner = await _userRepository.GetByIdAsync(request.OwnerId, cancellationToken);
        if (owner == null)
        {
            throw new InvalidOperationException($"Owner with ID '{request.OwnerId}' not found");
        }

        // Check if slug is already taken
        if (await _tenantRepository.ExistsBySlugAsync(request.Slug, cancellationToken))
        {
            throw new InvalidOperationException($"Tenant with slug '{request.Slug}' already exists");
        }

        // Create tenant
        var tenant = new Tenant(
            request.Name,
            request.Slug,
            request.OwnerId,
            request.Plan);

        // Add owner as Owner role
        tenant.AddUser(request.OwnerId, "Owner");

        await _tenantRepository.AddAsync(tenant, cancellationToken);

        // Publish integration event
        await _eventPublisher.PublishAsync(
            new TenantCreatedEvent(
                tenant.Id.ToString(),
                tenant.Id,
                tenant.Name,
                tenant.Slug,
                request.OwnerId,
                DateTime.UtcNow),
            cancellationToken);

        _logger.LogInformation(
            "Tenant {TenantId} ({Name}) was created by user {OwnerId}",
            tenant.Id, tenant.Name, request.OwnerId);

        return tenant.Id;
    }
}
