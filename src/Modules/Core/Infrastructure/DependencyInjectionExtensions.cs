using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql.EntityFrameworkCore.PostgreSQL;
using WebSearchIndexing.Modules.Core.Application;
using WebSearchIndexing.Modules.Core.Infrastructure;

namespace WebSearchIndexing.Modules.Core.Infrastructure;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddCoreInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        var connectionString = configuration.GetConnectionString("IndexingDb");

        services.AddPooledDbContextFactory<CoreDbContext>(options =>
        {
            options.UseNpgsql(connectionString, sqlOptions =>
            {
                sqlOptions.MigrationsAssembly(typeof(CoreDbContext).Assembly.FullName);
                sqlOptions.ExecutionStrategy(c => new NpgsqlRetryingExecutionStrategy(c, 4));
            });
        });

        services.AddScoped<ISettingsRepository, EfSettingsRepository>();

        return services;
    }
}
