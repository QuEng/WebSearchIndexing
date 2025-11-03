using System.Reflection;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor;
using WebSearchIndexing.BuildingBlocks.Web;
using WebSearchIndexing.BuildingBlocks.Web.Navigation;
using WebSearchIndexing.Modules.Reporting.Ui.Services;

namespace WebSearchIndexing.Modules.Reporting.Ui;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddReportingUiModule(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddSingleton<IRazorComponentAssemblyProvider, ReportingUiAssemblyProvider>();
        services.AddSingleton<INavigationContributor, ReportingNavigationContributor>();
        
        // Add specific HTTP client for Reporting API
        services.AddScoped<IReportingHttpClient, ReportingHttpClient>();

        return services;
    }

    private sealed class ReportingUiAssemblyProvider : IRazorComponentAssemblyProvider
    {
        public Assembly Assembly => typeof(AssemblyMarker).Assembly;
    }

    private sealed class ReportingNavigationContributor : INavigationContributor
    {
        public void Configure(NavigationBuilder builder)
        {
            builder.AddLink("Dashboard", Icons.Material.Filled.Dashboard, "/", NavLinkMatch.All, order: 0);
        }
    }
}

