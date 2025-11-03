using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using WebSearchIndexing.Modules.Catalog.Application;

namespace WebSearchIndexing.Modules.Catalog.Api;

public static class CatalogModule
{
    public static IServiceCollection AddCatalogModule(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        services.AddCatalogApplication();
        return services;
    }

    public static IEndpointRouteBuilder MapCatalogModuleEndpoints(this IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);

        var catalogGroup = endpoints.MapGroup("/api/v1/catalog");

        catalogGroup
            .MapSitesEndpoints()
            .MapServiceAccountsEndpoints()
            .MapBatchesEndpoints()
            .MapUrlsEndpoints();

        return endpoints;
    }
}
