using System.Reflection;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using WebSearchIndexing.BuildingBlocks.Messaging.Outbox;
using Polly;

namespace WebSearchIndexing.BuildingBlocks.Messaging.Outbox;

/// <summary>
/// Default implementation of outbox dispatcher
/// </summary>
public class OutboxDispatcher : IOutboxDispatcher
{
    private readonly IOutboxRepository _outboxRepository;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<OutboxDispatcher> _logger;

    public OutboxDispatcher(
        IOutboxRepository outboxRepository,
        IServiceProvider serviceProvider,
        ILogger<OutboxDispatcher> logger)
    {
        _outboxRepository = outboxRepository ?? throw new ArgumentNullException(nameof(outboxRepository));
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task ProcessPendingMessagesAsync(CancellationToken cancellationToken = default)
    {
        var pendingMessages = await _outboxRepository.GetPendingMessagesAsync(100, cancellationToken);

        foreach (var message in pendingMessages)
        {
            await ProcessMessageAsync(message, cancellationToken);
        }
    }

    public async Task ProcessPendingMessagesAsync(string tenantId, CancellationToken cancellationToken = default)
    {
        var pendingMessages = await _outboxRepository.GetPendingMessagesAsync(tenantId, 100, cancellationToken);

        foreach (var message in pendingMessages)
        {
            await ProcessMessageAsync(message, cancellationToken);
        }
    }

    private async Task ProcessMessageAsync(OutboxMessage message, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Processing outbox message {MessageId} of type {MessageType}", 
                message.Id, message.Type);

            await DispatchMessageAsync(message, cancellationToken);

            message.MarkAsProcessed();
            await _outboxRepository.UpdateAsync(message, cancellationToken);

            _logger.LogInformation("Successfully processed outbox message {MessageId}", message.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process outbox message {MessageId}", message.Id);
            
            message.MarkAsFailed(ex.Message);
            await _outboxRepository.UpdateAsync(message, cancellationToken);

            // If we've reached max retries, log as critical
            if (message.RetryCount >= 3)
            {
                _logger.LogCritical("Outbox message {MessageId} has failed {RetryCount} times and will not be retried", 
                    message.Id, message.RetryCount);
            }
        }
    }

    private async Task DispatchMessageAsync(OutboxMessage message, CancellationToken cancellationToken)
    {
        var eventType = ResolveEventType(message.Type);
        if (eventType == null)
        {
            throw new InvalidOperationException($"Could not find type {message.Type}");
        }

        var integrationEvent = JsonSerializer.Deserialize(message.Data, eventType) as IIntegrationEvent;
        if (integrationEvent == null)
        {
            throw new InvalidOperationException($"Could not deserialize event of type {message.Type}");
        }

        // Find and invoke handlers
        var handlerType = typeof(IIntegrationEventHandler<>).MakeGenericType(eventType);
        var handlers = _serviceProvider.GetServices(handlerType);

        var handleMethod = handlerType.GetMethod("HandleAsync");
        if (handleMethod == null)
        {
            throw new InvalidOperationException($"Could not find HandleAsync method on {handlerType}");
        }

        foreach (var handler in handlers)
        {
            var task = (Task)handleMethod.Invoke(handler, new object[] { integrationEvent, cancellationToken })!;
            await task;
        }
    }

    private Type? ResolveEventType(string typeName)
    {
        // First try the simple approach (works with Assembly-qualified names)
        var type = Type.GetType(typeName);
        if (type != null)
        {
            return type;
        }

        // Extract just the type name part if it's an assembly-qualified name
        var simpleTypeName = typeName;
        var commaIndex = typeName.IndexOf(',');
        if (commaIndex > 0)
        {
            simpleTypeName = typeName.Substring(0, commaIndex).Trim();
        }

        // Try with simple type name
        type = Type.GetType(simpleTypeName);
        if (type != null)
        {
            return type;
        }

        // If not found, search in loaded assemblies by full name
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            type = assembly.GetType(simpleTypeName);
            if (type != null)
            {
                return type;
            }
        }

        // Last resort: search by name only (class name without namespace) in loaded assemblies
        var className = simpleTypeName.Split('.').Last();
        if (!string.IsNullOrEmpty(className))
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                try 
                {
                    var candidateTypes = new List<Type>();
                    foreach (var assemblyType in assembly.GetTypes())
                    {
                        if (assemblyType.Name == className && typeof(IIntegrationEvent).IsAssignableFrom(assemblyType))
                        {
                            candidateTypes.Add(assemblyType);
                        }
                    }

                    if (candidateTypes.Count == 1)
                    {
                        return candidateTypes[0];
                    }
                    else if (candidateTypes.Count > 1)
                    {
                        // If multiple types found, try to find exact match by full name
                        foreach (var candidateType in candidateTypes)
                        {
                            if (candidateType.FullName == simpleTypeName)
                            {
                                return candidateType;
                            }
                        }
                    }
                }
                catch
                {
                    // Skip assemblies that can't be loaded
                    continue;
                }
            }
        }

        return null;
    }
}
