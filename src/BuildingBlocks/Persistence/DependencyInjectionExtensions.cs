using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using WebSearchIndexing.BuildingBlocks.Messaging.Outbox;
using WebSearchIndexing.BuildingBlocks.Persistence.Repositories;

namespace WebSearchIndexing.BuildingBlocks.Persistence;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddPersistence(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        return services;
    }

    public static IServiceCollection AddOutboxRepository<TDbContext>(this IServiceCollection services)
        where TDbContext : DbContext
    {
        ArgumentNullException.ThrowIfNull(services);
        
        services.AddScoped<IOutboxRepository, EfOutboxRepository<TDbContext>>();
        
        return services;
    }
}
