using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql.EntityFrameworkCore.PostgreSQL;
using WebSearchIndexing.BuildingBlocks.Messaging;
using WebSearchIndexing.BuildingBlocks.Persistence;
using WebSearchIndexing.Modules.Core.Application.Abstractions;
using WebSearchIndexing.Modules.Core.Infrastructure.Persistence;
using WebSearchIndexing.Modules.Core.Infrastructure.Persistence.Repositories;
using WebSearchIndexing.Modules.Core.Infrastructure.Tenancy;

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

        services.AddSingleton<TenantIdSaveChangesInterceptor>();

        services.AddPooledDbContextFactory<CoreDbContext>((sp, options) =>
        {
            options.UseNpgsql(connectionString, sqlOptions =>
            {
                sqlOptions.MigrationsAssembly(typeof(CoreDbContext).Assembly.FullName);
                sqlOptions.ExecutionStrategy(c => new NpgsqlRetryingExecutionStrategy(c, 4));
            });

            options.AddInterceptors(sp.GetRequiredService<TenantIdSaveChangesInterceptor>());
        });

        // Register CoreDbContext as scoped for dependency injection (needed for EfOutboxRepository)
        services.AddScoped<CoreDbContext>(sp =>
        {
            var factory = sp.GetRequiredService<IDbContextFactory<CoreDbContext>>();
            return factory.CreateDbContext();
        });

        services.AddScoped<ISettingsRepository, EfSettingsRepository>();

        // Add messaging and outbox support
        services.AddMessaging();
        services.AddOutboxRepository<CoreDbContext>();

        return services;
    }
}
