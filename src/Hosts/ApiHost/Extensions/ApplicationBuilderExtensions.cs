using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using WebSearchIndexing.Modules.Catalog.Infrastructure.Persistence;
using WebSearchIndexing.Modules.Core.Infrastructure;

namespace WebSearchIndexing.Hosts.ApiHost.Extensions;

internal static class ApplicationBuilderExtensions
{
    public static void ApplyMigrations(this WebApplication app)
    {
        ArgumentNullException.ThrowIfNull(app);

        using var scope = app.Services.CreateScope();
        var catalogFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<CatalogDbContext>>();
        using var catalogContext = catalogFactory.CreateDbContext();
        catalogContext.Database.Migrate();

        var coreFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<CoreDbContext>>();
        using var coreContext = coreFactory.CreateDbContext();
        coreContext.Database.Migrate();
    }
}
