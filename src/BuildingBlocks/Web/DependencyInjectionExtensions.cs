using Microsoft.Extensions.DependencyInjection;
using WebSearchIndexing.BuildingBlocks.Web.Navigation;

namespace WebSearchIndexing.BuildingBlocks.Web;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddWebSupport(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddSingleton<INavigationService, NavigationService>();

        return services;
    }
}
