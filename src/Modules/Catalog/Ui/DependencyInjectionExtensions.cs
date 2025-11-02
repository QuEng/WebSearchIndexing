using System;
using System.Reflection;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor;
using WebSearchIndexing.BuildingBlocks.Web;
using WebSearchIndexing.BuildingBlocks.Web.Navigation;
using WebSearchIndexing.Modules.Catalog.Ui.Services;

namespace WebSearchIndexing.Modules.Catalog.Ui;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddCatalogUiModule(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddSingleton<IRazorComponentAssemblyProvider, CatalogUiAssemblyProvider>();
        services.AddSingleton<INavigationContributor, CatalogNavigationContributor>();
        
        // Add HTTP client for Catalog API
        services.AddHttpClient<ICatalogHttpClient, CatalogHttpClient>();

        return services;
    }

    private sealed class CatalogUiAssemblyProvider : IRazorComponentAssemblyProvider
    {
        public Assembly Assembly => typeof(AssemblyMarker).Assembly;
    }

    private sealed class CatalogNavigationContributor : INavigationContributor
    {
        public void Configure(NavigationBuilder builder)
        {
            builder.AddGroup("Urls", Icons.Material.Filled.Link, order: 100)
                .AddLink("All urls", Icons.Material.Filled.AddLink, "/all-urls")
                .AddLink("Processed urls", Icons.Material.Filled.PhonelinkRing, "/processed-urls");

            builder.AddLink("Service accounts", Icons.Material.Filled.Key, "/service-accounts", NavLinkMatch.All, order: 200);
            builder.AddLink("Batches", Icons.Material.Filled.Storage, "/batches", NavLinkMatch.All, order: 300);
        }
    }
}
