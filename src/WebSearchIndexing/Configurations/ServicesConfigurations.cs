using WebSearchIndexing.Data.Repositories;
using WebSearchIndexing.Domain.Repositories;

namespace WebSearchIndexing.Configurations;

public static class ServicesConfigurations
{
    public static void AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<IServiceAccountRepository, ServiceAccountRepository>()
                .AddScoped<IUrlRequestRepository, UrlRequestRepository>()
                .AddScoped<ISettingRepository, SettingRepository>();
    }
}