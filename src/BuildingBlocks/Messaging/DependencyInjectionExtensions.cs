using Microsoft.Extensions.DependencyInjection;
using WebSearchIndexing.BuildingBlocks.Messaging.BackgroundServices;
using WebSearchIndexing.BuildingBlocks.Messaging.Outbox;

namespace WebSearchIndexing.BuildingBlocks.Messaging;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddMessaging(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        
        // Core messaging services
        services.AddScoped<IIntegrationEventPublisher, OutboxIntegrationEventPublisher>();
        services.AddScoped<IOutboxDispatcher, OutboxDispatcher>();
        
        return services;
    }

    public static IServiceCollection AddOutboxBackgroundService(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        
        services.AddHostedService<OutboxBackgroundService>();
        
        return services;
    }

    public static IServiceCollection AddIntegrationEventHandler<TEvent, THandler>(this IServiceCollection services)
        where TEvent : class, IIntegrationEvent
        where THandler : class, IIntegrationEventHandler<TEvent>
    {
        ArgumentNullException.ThrowIfNull(services);
        
        services.AddScoped<IIntegrationEventHandler<TEvent>, THandler>();
        
        return services;
    }
}
