using WebSearchIndexing.BuildingBlocks.Messaging;

namespace WebSearchIndexing.Modules.Identity.Application.IntegrationEvents.External;

// Copy of Core Integration Events to avoid direct module dependencies
// These should be kept in sync with the original events in Core module

public class SettingsChangedEvent : IntegrationEvent
{
    public Guid SettingsId { get; }
    public string Key { get; }
    public int RequestsPerDay { get; }
    public bool IsEnabled { get; }

    public SettingsChangedEvent(string tenantId, Guid settingsId, string key, int requestsPerDay, bool isEnabled) 
        : base(tenantId)
    {
        SettingsId = settingsId;
        Key = key ?? throw new ArgumentNullException(nameof(key));
        RequestsPerDay = requestsPerDay;
        IsEnabled = isEnabled;
    }
}
