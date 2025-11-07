using Microsoft.Extensions.Logging;
using WebSearchIndexing.BuildingBlocks.Messaging;
using WebSearchIndexing.Modules.Identity.Application.IntegrationEvents.External;

namespace WebSearchIndexing.Modules.Identity.Application.IntegrationEventHandlers.Catalog;

/// <summary>
/// Handles the ServiceAccountUpdated event from the Catalog module
/// </summary>
public sealed class ServiceAccountUpdatedEventHandler : IIntegrationEventHandler<ServiceAccountUpdatedEvent>
{
    private readonly ILogger<ServiceAccountUpdatedEventHandler> _logger;

    public ServiceAccountUpdatedEventHandler(ILogger<ServiceAccountUpdatedEventHandler> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task HandleAsync(ServiceAccountUpdatedEvent integrationEvent, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(integrationEvent);

        _logger.LogInformation(
            "Identity module received ServiceAccountUpdated event for ServiceAccount {ServiceAccountId} " +
            "in project {ProjectId} for tenant {TenantId}",
            integrationEvent.ServiceAccountId,
            integrationEvent.ProjectId,
            integrationEvent.TenantId);

        // Business logic for Identity module when a service account is updated
        // For example:
        // - Update security audit logs
        // - Refresh user permissions if quota changes affect access levels
        // - Update tenant configuration if necessary

        _logger.LogInformation(
            "Security audit: Service account {ServiceAccountId} was updated in project {ProjectId} " +
            "with new quota limit {QuotaLimit} for tenant {TenantId}",
            integrationEvent.ServiceAccountId,
            integrationEvent.ProjectId,
            integrationEvent.QuotaLimitPerDay,
            integrationEvent.TenantId);

        await Task.CompletedTask;
    }
}
