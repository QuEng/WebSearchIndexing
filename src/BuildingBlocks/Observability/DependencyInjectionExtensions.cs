using System;
using Microsoft.Extensions.DependencyInjection;

namespace WebSearchIndexing.BuildingBlocks.Observability;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddObservability(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        return services;
    }
}
