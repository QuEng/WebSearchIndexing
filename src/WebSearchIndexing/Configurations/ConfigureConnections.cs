using Microsoft.EntityFrameworkCore;
using Npgsql.EntityFrameworkCore.PostgreSQL;
using WebSearchIndexing.Data;

namespace WebSearchIndexing.Configurations;

public static class ConfigureConnections
{
    public static IServiceCollection AddConnectionProvider(this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("IndexingDb");

        services.AddPooledDbContextFactory<IndexingDbContext>(dbContextOptions =>
        {
            dbContextOptions
            .UseNpgsql(
                connectionString: connectionString,
                sqlOptions =>
                {
                    sqlOptions.MigrationsAssembly("WebSearchIndexing.Data");
                    sqlOptions.ExecutionStrategy(c => new NpgsqlRetryingExecutionStrategy(c, 4));
                });
        });

        return services;
    }
}