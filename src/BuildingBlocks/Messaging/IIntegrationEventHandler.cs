namespace WebSearchIndexing.BuildingBlocks.Messaging;

/// <summary>
/// Handler for integration events
/// </summary>
public interface IIntegrationEventHandler<in TEvent> where TEvent : IIntegrationEvent
{
    /// <summary>
    /// Handle the integration event
    /// </summary>
    Task HandleAsync(TEvent integrationEvent, CancellationToken cancellationToken = default);
}
