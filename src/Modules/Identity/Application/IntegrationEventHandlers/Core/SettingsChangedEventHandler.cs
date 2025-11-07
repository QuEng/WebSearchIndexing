using Microsoft.Extensions.Logging;
using WebSearchIndexing.BuildingBlocks.Messaging;
using WebSearchIndexing.Modules.Identity.Application.IntegrationEvents.External;

namespace WebSearchIndexing.Modules.Identity.Application.IntegrationEventHandlers.Core;

/// <summary>
/// Handles the SettingsChanged event from the Core module
/// </summary>
public sealed class SettingsChangedEventHandler : IIntegrationEventHandler<SettingsChangedEvent>
{
    private readonly ILogger<SettingsChangedEventHandler> _logger;

    public SettingsChangedEventHandler(ILogger<SettingsChangedEventHandler> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task HandleAsync(SettingsChangedEvent integrationEvent, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(integrationEvent);

        _logger.LogInformation(
            "Identity module received SettingsChanged event for setting {Key} (ID: {SettingsId}) " +
            "for tenant {TenantId}. New values: RequestsPerDay={RequestsPerDay}, IsEnabled={IsEnabled}",
            integrationEvent.Key,
            integrationEvent.SettingsId,
            integrationEvent.TenantId,
            integrationEvent.RequestsPerDay,
            integrationEvent.IsEnabled);

        // Business logic for Identity module when core settings change
        // For example:
        // - Update rate limiting rules for authentication endpoints based on RequestsPerDay
        // - Disable/enable certain identity features based on IsEnabled flag
        // - Update tenant-specific security policies
        // - Refresh cached authentication configuration

        if (integrationEvent.Key.Contains("Authentication", StringComparison.OrdinalIgnoreCase) ||
            integrationEvent.Key.Contains("Security", StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogInformation(
                "Security-related setting {Key} changed for tenant {TenantId}. " +
                "Identity module may need to refresh security policies.",
                integrationEvent.Key,
                integrationEvent.TenantId);
        }

        await Task.CompletedTask;
    }
}
