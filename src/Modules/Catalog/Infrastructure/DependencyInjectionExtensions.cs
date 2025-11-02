using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql.EntityFrameworkCore.PostgreSQL;
using WebSearchIndexing.Modules.Catalog.Application.Abstractions;
using WebSearchIndexing.Modules.Catalog.Infrastructure.Persistence;
using WebSearchIndexing.Modules.Catalog.Infrastructure.Persistence.Repositories;

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

        services.AddPooledDbContextFactory<CatalogDbContext>(options =>
        {
            options.UseNpgsql(connectionString, sqlOptions =>
            {
                sqlOptions.MigrationsAssembly(typeof(CatalogDbContext).Assembly.FullName);
                sqlOptions.ExecutionStrategy(c => new NpgsqlRetryingExecutionStrategy(c, 4));
            });
        });

        services.AddScoped<IServiceAccountRepository, EfServiceAccountRepository>();
        services.AddScoped<IUrlRequestRepository, EfUrlRequestRepository>();
        services.AddScoped<ICatalogRepository, CatalogRepository>();

        return services;
    }
}
