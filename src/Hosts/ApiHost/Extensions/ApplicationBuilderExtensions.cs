using Microsoft.EntityFrameworkCore;
using WebSearchIndexing.Modules.Catalog.Infrastructure.Persistence;
using WebSearchIndexing.Modules.Core.Infrastructure.Persistence;
using WebSearchIndexing.Modules.Identity.Infrastructure.Persistence;

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

        var identityFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<IdentityDbContext>>();
        using var identityContext = identityFactory.CreateDbContext();
        identityContext.Database.Migrate();
    }
}
