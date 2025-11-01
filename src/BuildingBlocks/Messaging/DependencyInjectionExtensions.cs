using System;
using Microsoft.Extensions.DependencyInjection;

namespace WebSearchIndexing.BuildingBlocks.Messaging;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddMessaging(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        return services;
    }
}
