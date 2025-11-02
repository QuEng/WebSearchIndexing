using System.Text.Json;
using WebSearchIndexing.BuildingBlocks.Messaging.Outbox;

namespace WebSearchIndexing.BuildingBlocks.Messaging;

/// <summary>
/// Default implementation of integration event publisher using outbox pattern
/// </summary>
public class OutboxIntegrationEventPublisher : IIntegrationEventPublisher
{
    private readonly IOutboxRepository _outboxRepository;

    public OutboxIntegrationEventPublisher(IOutboxRepository outboxRepository)
    {
        _outboxRepository = outboxRepository ?? throw new ArgumentNullException(nameof(outboxRepository));
    }

    public async Task PublishAsync(IIntegrationEvent integrationEvent, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(integrationEvent);

        var message = CreateOutboxMessage(integrationEvent);
        await _outboxRepository.AddAsync(message, cancellationToken);
    }

    public async Task PublishAsync(IEnumerable<IIntegrationEvent> integrationEvents, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(integrationEvents);

        foreach (var integrationEvent in integrationEvents)
        {
            var message = CreateOutboxMessage(integrationEvent);
            await _outboxRepository.AddAsync(message, cancellationToken);
        }
    }

    private static OutboxMessage CreateOutboxMessage(IIntegrationEvent integrationEvent)
    {
        var eventType = integrationEvent.GetType();
        var assemblyQualifiedName = eventType.AssemblyQualifiedName ?? eventType.FullName ?? throw new InvalidOperationException("Event type name is null");
        var eventData = JsonSerializer.Serialize(integrationEvent, eventType);

        return new OutboxMessage(
            integrationEvent.Id,
            Guid.Parse(integrationEvent.TenantId),
            assemblyQualifiedName,
            eventData,
            integrationEvent.OccurredOn);
    }
}
