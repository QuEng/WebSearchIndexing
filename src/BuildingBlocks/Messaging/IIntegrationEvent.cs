namespace WebSearchIndexing.BuildingBlocks.Messaging;

/// <summary>
/// Marker interface for integration events that can be published across modules
/// </summary>
public interface IIntegrationEvent
{
    /// <summary>
    /// Unique identifier for the event
    /// </summary>
    Guid Id { get; }
    
    /// <summary>
    /// When the event occurred
    /// </summary>
    DateTime OccurredOn { get; }
    
    /// <summary>
    /// The tenant context for this event
    /// </summary>
    string TenantId { get; }
}
