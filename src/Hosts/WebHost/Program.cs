using Finbuckle.MultiTenant;
using MudBlazor.Services;
using Serilog;
using WebSearchIndexing.BuildingBlocks.Observability;
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
using WebSearchIndexing.Modules.Reporting.Ui;
using WebSearchIndexing.Modules.Submission.Api;
using HealthChecks.NpgSql;
using Microsoft.AspNetCore.DataProtection;

var builder = WebApplication.CreateBuilder(args);

// Serilog basic setup
Log.Logger = new LoggerConfiguration()
 .ReadFrom.Configuration(builder.Configuration)
 .Enrich.FromLogContext()
 .WriteTo.Console()
 .CreateLogger();

builder.Host.UseSerilog();

builder.Services.AddHealthChecks()
 .AddNpgSql(builder.Configuration.GetConnectionString("IndexingDb")!, name: "postgres");

builder.Services.AddWebSupport();
builder.Services.AddMudServices();

// Data Protection (for secrets encryption)
builder.Services.AddDataProtection();

// Observability (OTEL)
builder.Services.AddObservability();

var connectionString = builder.Configuration.GetConnectionString("IndexingDb");

builder.Services
 .AddMultiTenant<TenantInfo>()
 .WithInMemoryStore(options =>
 {
 options.Tenants.Add(new TenantInfo
 {
 Id = Guid.Empty.ToString(),
 Identifier = "default",
 Name = "Default",
 ConnectionString = connectionString
 });
 })
 .WithStaticStrategy("default");

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
 .AddCatalogUiModule()
 .AddReportingUiModule();

var app = builder.Build();

app.ApplyMigrations();
app.UseMultiTenant();

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
// Readiness endpoint (extend with checks later)
app.MapHealthChecks("/health/ready");

app.MapCoreModuleEndpoints();
app.MapCatalogModuleEndpoints();
app.MapSubmissionModuleEndpoints();
app.MapInspectionModuleEndpoints();
app.MapCrawlerModuleEndpoints();
app.MapNotificationsModuleEndpoints();
app.MapReportingModuleEndpoints();

app.Run();
