using System;
using Microsoft.Extensions.DependencyInjection;

namespace WebSearchIndexing.BuildingBlocks.Abstractions;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddAbstractions(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        return services;
    }
}
