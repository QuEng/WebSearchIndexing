namespace WebSearchIndexing.BuildingBlocks.Messaging.Outbox;

/// <summary>
/// Represents an outbox message entry in the database
/// </summary>
public class OutboxMessage
{
    public Guid Id { get; private set; }
    public Guid TenantId { get; private set; }
    public string Type { get; private set; } = string.Empty;
    public string Data { get; private set; } = string.Empty;
    public DateTime OccurredOn { get; private set; }
    public DateTime? ProcessedOn { get; private set; }
    public OutboxMessageStatus Status { get; private set; }
    public string? Error { get; private set; }
    public int RetryCount { get; private set; }

    // EF Core constructor
    private OutboxMessage() { }

    public OutboxMessage(
        Guid id,
        Guid tenantId,
        string type,
        string data,
        DateTime occurredOn)
    {
        Id = id;
        TenantId = tenantId;
        Type = type ?? throw new ArgumentNullException(nameof(type));
        Data = data ?? throw new ArgumentNullException(nameof(data));
        OccurredOn = occurredOn;
        Status = OutboxMessageStatus.Pending;
        RetryCount = 0;
    }

    public void MarkAsProcessed()
    {
        Status = OutboxMessageStatus.Processed;
        ProcessedOn = DateTime.UtcNow;
        Error = null;
    }

    public void MarkAsFailed(string error)
    {
        Status = OutboxMessageStatus.Failed;
        Error = error;
        RetryCount++;
    }

    public void MarkAsRetrying()
    {
        Status = OutboxMessageStatus.Pending;
        Error = null;
    }
}

public enum OutboxMessageStatus
{
    Pending = 0,
    Processed = 1,
    Failed = 2
}
