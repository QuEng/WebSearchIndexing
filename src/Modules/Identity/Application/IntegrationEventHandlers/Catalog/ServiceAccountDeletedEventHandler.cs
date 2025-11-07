using Microsoft.Extensions.Logging;
using WebSearchIndexing.BuildingBlocks.Messaging;
using WebSearchIndexing.Modules.Identity.Application.IntegrationEvents.External;

namespace WebSearchIndexing.Modules.Identity.Application.IntegrationEventHandlers.Catalog;

/// <summary>
/// Handles the ServiceAccountDeleted event from the Catalog module
/// </summary>
public sealed class ServiceAccountDeletedEventHandler : IIntegrationEventHandler<ServiceAccountDeletedEvent>
{
    private readonly ILogger<ServiceAccountDeletedEventHandler> _logger;

    public ServiceAccountDeletedEventHandler(ILogger<ServiceAccountDeletedEventHandler> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task HandleAsync(ServiceAccountDeletedEvent integrationEvent, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(integrationEvent);

        _logger.LogInformation(
            "Identity module received ServiceAccountDeleted event for ServiceAccount {ServiceAccountId} " +
            "for tenant {TenantId}",
            integrationEvent.ServiceAccountId,
            integrationEvent.TenantId);

        // Business logic for Identity module when a service account is deleted
        // For example:
        // - Revoke user permissions related to this service account
        // - Clean up any identity-related resources tied to this service account
        // - Log security audit event for service account deletion
        // - Update tenant configuration to remove access grants

        _logger.LogWarning(
            "Security audit: Service account {ServiceAccountId} was deleted for tenant {TenantId}. " +
            "All related permissions should be revoked.",
            integrationEvent.ServiceAccountId,
            integrationEvent.TenantId);

        await Task.CompletedTask;
    }
}
