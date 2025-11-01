using System;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace WebSearchIndexing.Modules.Crawler.Api;

public static class CrawlerModule
{
    public static IServiceCollection AddCrawlerModule(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        return services;
    }

    public static IEndpointRouteBuilder MapCrawlerModuleEndpoints(this IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);
        return endpoints;
    }
}
