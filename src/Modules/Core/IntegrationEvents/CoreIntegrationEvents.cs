using WebSearchIndexing.BuildingBlocks.Messaging;

namespace WebSearchIndexing.Modules.Core.IntegrationEvents;

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
