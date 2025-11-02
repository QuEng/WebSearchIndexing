namespace WebSearchIndexing.BuildingBlocks.Messaging.Outbox;

/// <summary>
/// Service for processing outbox messages
/// </summary>
public interface IOutboxDispatcher
{
    /// <summary>
    /// Process pending outbox messages
    /// </summary>
    Task ProcessPendingMessagesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Process pending outbox messages for a specific tenant
    /// </summary>
    Task ProcessPendingMessagesAsync(string tenantId, CancellationToken cancellationToken = default);
}
