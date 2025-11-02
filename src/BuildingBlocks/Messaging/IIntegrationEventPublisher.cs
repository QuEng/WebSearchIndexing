namespace WebSearchIndexing.BuildingBlocks.Messaging;

/// <summary>
/// Service for publishing integration events within the same transaction
/// </summary>
public interface IIntegrationEventPublisher
{
    /// <summary>
    /// Publishes an integration event by storing it in the outbox
    /// </summary>
    Task PublishAsync(IIntegrationEvent integrationEvent, CancellationToken cancellationToken = default);

    /// <summary>
    /// Publishes multiple integration events by storing them in the outbox
    /// </summary>
    Task PublishAsync(IEnumerable<IIntegrationEvent> integrationEvents, CancellationToken cancellationToken = default);
}
