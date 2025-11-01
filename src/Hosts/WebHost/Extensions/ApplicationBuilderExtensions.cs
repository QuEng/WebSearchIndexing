using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using WebSearchIndexing.Data;

namespace WebSearchIndexing.Hosts.WebHost.Extensions;

internal static class ApplicationBuilderExtensions
{
    public static void ApplyMigrations(this WebApplication app)
    {
        ArgumentNullException.ThrowIfNull(app);

        using var scope = app.Services.CreateScope();
        var factory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<IndexingDbContext>>();
        using var context = factory.CreateDbContext();
        context.Database.Migrate();
    }
}
