using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WebSearchIndexing.Modules.Catalog.Infrastructure;
using WebSearchIndexing.Modules.Core.Infrastructure;

namespace WebSearchIndexing.Hosts.WebHost.Extensions;

internal static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services
            .AddCatalogInfrastructure(configuration)
            .AddCoreInfrastructure(configuration);

        return services;
    }
}
