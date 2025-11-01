using System;
using Microsoft.Extensions.DependencyInjection;

namespace WebSearchIndexing.BuildingBlocks.Persistence;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddPersistence(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        return services;
    }
}
