using MudBlazor.Services;
using WebSearchIndexing.BuildingBlocks.Web;
using WebSearchIndexing.BuildingBlocks.Web.Navigation;
using WebSearchIndexing.Hosts.WebHost.Components;
using WebSearchIndexing.Hosts.WebHost.Extensions;
using WebSearchIndexing.Hosts.WebHost.Navigation;
using WebSearchIndexing.Modules.Catalog.Api;
using WebSearchIndexing.Modules.Catalog.Ui;
using WebSearchIndexing.Modules.Core.Api;
using WebSearchIndexing.Modules.Core.Application;
using WebSearchIndexing.Modules.Core.Ui;
using WebSearchIndexing.Modules.Crawler.Api;
using WebSearchIndexing.Modules.Inspection.Api;
using WebSearchIndexing.Modules.Notifications.Api;
using WebSearchIndexing.Modules.Reporting.Api;
using WebSearchIndexing.Modules.Submission.Api;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHealthChecks();
builder.Services.AddWebSupport();
builder.Services.AddMudServices();

builder.Services
    .AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddSingleton<INavigationContributor, WebHostNavigationContributor>();

builder.Services
    .AddCoreModule()
    .AddCatalogModule()
    .AddSubmissionModule()
    .AddInspectionModule()
    .AddCrawlerModule()
    .AddNotificationsModule()
    .AddReportingModule()
    .AddCoreApplicationModule()
    .AddCoreUiModule()
    .AddCatalogUiModule();

var app = builder.Build();

app.ApplyMigrations();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseStaticFiles();
app.UseAntiforgery();

var componentAssemblies = app.Services
    .GetRequiredService<IEnumerable<IRazorComponentAssemblyProvider>>()
    .Select(provider => provider.Assembly)
    .Where(assembly => assembly != typeof(App).Assembly)
    .ToArray();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddAdditionalAssemblies(componentAssemblies);

app.MapHealthChecks("/health/live");

app.MapCoreModuleEndpoints();
app.MapCatalogModuleEndpoints();
app.MapSubmissionModuleEndpoints();
app.MapInspectionModuleEndpoints();
app.MapCrawlerModuleEndpoints();
app.MapNotificationsModuleEndpoints();
app.MapReportingModuleEndpoints();

app.Run();
