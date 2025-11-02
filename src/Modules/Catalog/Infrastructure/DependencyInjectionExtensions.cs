using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql.EntityFrameworkCore.PostgreSQL;
using WebSearchIndexing.Modules.Catalog.Application.Abstractions;
using WebSearchIndexing.Modules.Catalog.Infrastructure.Persistence;
using WebSearchIndexing.Modules.Catalog.Infrastructure.Persistence.Repositories;
using WebSearchIndexing.Modules.Catalog.Infrastructure.Persistence.Interceptors;

namespace WebSearchIndexing.Modules.Catalog.Infrastructure;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddCatalogInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        var connectionString = configuration.GetConnectionString("IndexingDb");

        services.AddSingleton<ServiceAccountCredentialsEncryptionInterceptor>();
        services.AddSingleton<ServiceAccountCredentialsDecryptionInterceptor>();
        services.AddSingleton<TenantIdSaveChangesInterceptor>();

        services.AddPooledDbContextFactory<CatalogDbContext>((sp, options) =>
        {
            options.UseNpgsql(connectionString, sqlOptions =>
            {
                sqlOptions.MigrationsAssembly(typeof(CatalogDbContext).Assembly.FullName);
                sqlOptions.ExecutionStrategy(c => new NpgsqlRetryingExecutionStrategy(c, 4));
            });

            options.AddInterceptors(
                sp.GetRequiredService<ServiceAccountCredentialsEncryptionInterceptor>(),
                sp.GetRequiredService<ServiceAccountCredentialsDecryptionInterceptor>(),
                sp.GetRequiredService<TenantIdSaveChangesInterceptor>());
        });

        services.AddScoped<IServiceAccountRepository, EfServiceAccountRepository>();
        services.AddScoped<IUrlRequestRepository, EfUrlRequestRepository>();
        services.AddScoped<ICatalogRepository, CatalogRepository>();

        return services;
    }
}
