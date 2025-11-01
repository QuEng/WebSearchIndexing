using Microsoft.EntityFrameworkCore;
using MudBlazor.Services;
using WebSearchIndexing.Configurations;
using WebSearchIndexing.Data;
using WebSearchIndexing.Modules.Core.Application;
using WebSearchIndexing.Pages;

namespace WebSearchIndexing.Extensions;

public static class HostingExtensions
{
    public static WebApplication ConfigureServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddRazorComponents()
            .AddInteractiveServerComponents();

        Thread.Sleep(10000); // Wait for the database to start
        builder.Services.AddConnectionProvider(builder.Configuration);

        builder.Services.AddRepositories();

        builder.Services.AddMudServices();

        builder.Services.AddCoreApplicationModule();

        return builder.Build();
    }

    private static void ApplyMigrations(this WebApplication app)
    {
        using var serviceScope = app.Services.CreateScope();
        var factory = serviceScope.ServiceProvider.GetRequiredService<IDbContextFactory<IndexingDbContext>>();
        using var context = factory.CreateDbContext();
        context.Database.Migrate();
    }

    public static WebApplication ConfigurePipeline(this WebApplication app)
    {
        app.ApplyMigrations();

        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Error", createScopeForErrors: true);
            app.UseHsts();
        }

        app.UseStaticFiles();
        app.UseAntiforgery();

        app.MapRazorComponents<App>()
            .AddInteractiveServerRenderMode();

        return app;
    }
}
