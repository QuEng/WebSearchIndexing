using Microsoft.Extensions.DependencyInjection;
using WebSearchIndexing.Modules.Catalog.Application.Commands.ServiceAccounts;
using WebSearchIndexing.Modules.Catalog.Application.Commands.Sites.CreateSite;
using WebSearchIndexing.Modules.Catalog.Application.Commands.Urls.Import;
using WebSearchIndexing.Modules.Catalog.Application.Commands.Urls.UpdateStatus;

namespace WebSearchIndexing.Modules.Catalog.Application;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddCatalogApplication(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddScoped<CreateSiteHandler>();
        services.AddScoped<AddServiceAccountHandler>();
        services.AddScoped<ImportUrlsHandler>();
        services.AddScoped<UpdateUrlStatusHandler>();

        return services;
    }
}
