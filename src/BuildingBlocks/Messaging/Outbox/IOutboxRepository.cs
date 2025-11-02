using WebSearchIndexing.BuildingBlocks.Messaging.Outbox;

namespace WebSearchIndexing.BuildingBlocks.Messaging.Outbox;

/// <summary>
/// Repository interface for managing outbox messages
/// </summary>
public interface IOutboxRepository
{
    /// <summary>
    /// Add a new outbox message
    /// </summary>
    Task AddAsync(OutboxMessage message, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get pending messages for processing
    /// </summary>
    Task<IEnumerable<OutboxMessage>> GetPendingMessagesAsync(
        int batchSize = 100, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get pending messages for a specific tenant
    /// </summary>
    Task<IEnumerable<OutboxMessage>> GetPendingMessagesAsync(
        string tenantId,
        int batchSize = 100,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Update message status
    /// </summary>
    Task UpdateAsync(OutboxMessage message, CancellationToken cancellationToken = default);

    /// <summary>
    /// Remove old processed messages (cleanup)
    /// </summary>
    Task CleanupProcessedMessagesAsync(
        DateTime beforeDate,
        CancellationToken cancellationToken = default);
}
