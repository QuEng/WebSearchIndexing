using Microsoft.Extensions.Logging;
using WebSearchIndexing.BuildingBlocks.Messaging;
using WebSearchIndexing.Modules.Identity.Application.IntegrationEvents.External;

namespace WebSearchIndexing.Modules.Identity.Application.IntegrationEventHandlers.Catalog;

/// <summary>
/// Handles the ServiceAccountCreated event from the Catalog module
/// </summary>
public sealed class ServiceAccountCreatedEventHandler : IIntegrationEventHandler<ServiceAccountCreatedEvent>
{
    private readonly ILogger<ServiceAccountCreatedEventHandler> _logger;

    public ServiceAccountCreatedEventHandler(ILogger<ServiceAccountCreatedEventHandler> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task HandleAsync(ServiceAccountCreatedEvent integrationEvent, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(integrationEvent);

        _logger.LogInformation(
            "Identity module received ServiceAccountCreated event for ServiceAccount {ServiceAccountId} " +
            "in project {ProjectId} for tenant {TenantId}",
            integrationEvent.ServiceAccountId,
            integrationEvent.ProjectId,
            integrationEvent.TenantId);

        // Here you can implement business logic for Identity module when a service account is created
        // For example:
        // - Update tenant permissions if service account affects tenant access
        // - Log security audit event for service account creation
        // - Update user permissions for service account management

        // Example: Log security event
        _logger.LogInformation(
            "Security audit: Service account {ServiceAccountId} was created in project {ProjectId} " +
            "with quota limit {QuotaLimit} for tenant {TenantId}",
            integrationEvent.ServiceAccountId,
            integrationEvent.ProjectId,
            integrationEvent.QuotaLimitPerDay,
            integrationEvent.TenantId);

        await Task.CompletedTask;
    }
}
