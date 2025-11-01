using System.Reflection;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor;
using WebSearchIndexing.BuildingBlocks.Web;
using WebSearchIndexing.BuildingBlocks.Web.Navigation;

namespace WebSearchIndexing.Modules.Core.Ui;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddCoreUiModule(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddSingleton<IRazorComponentAssemblyProvider, CoreUiAssemblyProvider>();
        services.AddSingleton<INavigationContributor, CoreNavigationContributor>();

        return services;
    }

    private sealed class CoreUiAssemblyProvider : IRazorComponentAssemblyProvider
    {
        public Assembly Assembly => typeof(AssemblyMarker).Assembly;
    }

    private sealed class CoreNavigationContributor : INavigationContributor
    {
        public void Configure(NavigationBuilder builder)
        {
            builder.AddLink("Settings", Icons.Material.Filled.Settings, "/settings", NavLinkMatch.All, order: 10);
        }
    }
}
