using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql.EntityFrameworkCore.PostgreSQL;
using WebSearchIndexing.Data;
using WebSearchIndexing.Data.Repositories;
using WebSearchIndexing.Domain.Repositories;

namespace WebSearchIndexing.Hosts.WebHost.Extensions;

internal static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        var connectionString = configuration.GetConnectionString("IndexingDb");

        services.AddPooledDbContextFactory<IndexingDbContext>(options =>
        {
            options.UseNpgsql(connectionString, sqlOptions =>
            {
                sqlOptions.MigrationsAssembly(typeof(IndexingDbContext).Assembly.FullName);
                sqlOptions.ExecutionStrategy(c => new NpgsqlRetryingExecutionStrategy(c, 4));
            });
        });

        services.AddScoped<IServiceAccountRepository, ServiceAccountRepository>();
        services.AddScoped<IUrlRequestRepository, UrlRequestRepository>();
        services.AddScoped<ISettingRepository, SettingRepository>();

        return services;
    }
}
