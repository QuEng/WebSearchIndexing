using Microsoft.Extensions.DependencyInjection;
using WebSearchIndexing.Modules.Catalog.Application.Commands.ServiceAccounts;
using WebSearchIndexing.Modules.Catalog.Application.Commands.ServiceAccounts.Update;
using WebSearchIndexing.Modules.Catalog.Application.Commands.Sites.CreateSite;
using WebSearchIndexing.Modules.Catalog.Application.Commands.Urls.Import;
using WebSearchIndexing.Modules.Catalog.Application.Commands.Urls.UpdateStatus;
using WebSearchIndexing.Modules.Catalog.Application.Commands.Urls.Update;
using WebSearchIndexing.Modules.Catalog.Application.Commands.Urls.Delete;
using WebSearchIndexing.Modules.Catalog.Application.Commands.Urls.DeleteBatch;
using WebSearchIndexing.Modules.Catalog.Application.Queries.Urls;
using WebSearchIndexing.Modules.Catalog.Application.Queries.ServiceAccounts;

namespace WebSearchIndexing.Modules.Catalog.Application;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddCatalogApplication(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        // Command handlers
        services.AddScoped<CreateSiteHandler>();
        services.AddScoped<AddServiceAccountHandler>();
        services.AddScoped<UpdateServiceAccountHandler>();
        services.AddScoped<ImportUrlsHandler>();
        services.AddScoped<UpdateUrlStatusHandler>();
        services.AddScoped<UpdateUrlHandler>();
        services.AddScoped<DeleteUrlHandler>();
        services.AddScoped<DeleteUrlsBatchHandler>();
        
        // Query handlers
        services.AddScoped<GetUrlsHandler>();
        services.AddScoped<GetUrlsCountHandler>();
        services.AddScoped<GetServiceAccountsHandler>();
        services.AddScoped<GetServiceAccountByIdHandler>();
        services.AddScoped<CheckServiceAccountExistsHandler>();

        return services;
    }
}
