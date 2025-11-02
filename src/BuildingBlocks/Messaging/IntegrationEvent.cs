namespace WebSearchIndexing.BuildingBlocks.Messaging;

/// <summary>
/// Base class for integration events
/// </summary>
public abstract class IntegrationEvent : IIntegrationEvent
{
    public Guid Id { get; private set; }
    public DateTime OccurredOn { get; private set; }
    public string TenantId { get; private set; }

    protected IntegrationEvent(string tenantId)
    {
        Id = Guid.NewGuid();
        OccurredOn = DateTime.UtcNow;
        TenantId = tenantId ?? throw new ArgumentNullException(nameof(tenantId));
    }

    protected IntegrationEvent(Guid id, DateTime occurredOn, string tenantId)
    {
        Id = id;
        OccurredOn = occurredOn;
        TenantId = tenantId ?? throw new ArgumentNullException(nameof(tenantId));
    }
}
