using System;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace WebSearchIndexing.Modules.Core.Api;

public static class CoreModule
{
    public static IServiceCollection AddCoreModule(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        return services;
    }

    public static IEndpointRouteBuilder MapCoreModuleEndpoints(this IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);
        return endpoints;
    }
}
