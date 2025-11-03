using Microsoft.AspNetCore.Components.Routing;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor;
using System.Reflection;
using WebSearchIndexing.BuildingBlocks.Web;
using WebSearchIndexing.BuildingBlocks.Web.Navigation;
using WebSearchIndexing.Modules.Catalog.Ui.Services;
using WebSearchIndexing.Contracts.Catalog;

namespace WebSearchIndexing.Modules.Catalog.Ui;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddCatalogUiModule(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddSingleton<IRazorComponentAssemblyProvider, CatalogUiAssemblyProvider>();
        services.AddSingleton<INavigationContributor, CatalogNavigationContributor>();

        // Add HttpClient for API services
        services.AddScoped<IUrlsApiService, UrlsApiService>();
        services.AddScoped<IServiceAccountsApiService, ServiceAccountsApiService>();

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
            builder
                .AddGroup("Urls", Icons.Material.Filled.Link, order: 100)
                .AddLink("All urls", Icons.Material.Filled.AddLink, "/all-urls")
                .AddLink("Processed urls", Icons.Material.Filled.PhonelinkRing, "/processed-urls");

            builder.AddLink("Service accounts", Icons.Material.Filled.Key, "/service-accounts", NavLinkMatch.All, order: 200);
            builder.AddLink("Batches", Icons.Material.Filled.Storage, "/batches", NavLinkMatch.All, order: 300);
        }
    }
}
