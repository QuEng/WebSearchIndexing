using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;
using WebSearchIndexing.BuildingBlocks.Web;
using WebSearchIndexing.Hosts.WasmHost;
using WebSearchIndexing.Modules.Catalog.Ui;
using WebSearchIndexing.Modules.Core.Ui;
using WebSearchIndexing.Modules.Identity.Ui;
using WebSearchIndexing.Modules.Reporting.Ui;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Configure API base address
var apiBaseAddress = "http://localhost:5093/";

// Configure HttpClient with hybrid authentication support
builder.Services.AddIdentityHttpClient(apiBaseAddress);

// Legacy HttpClient for backward compatibility
builder.Services.AddHttpClient("WebSearchIndexing.Api", client =>
{
    client.BaseAddress = new Uri(apiBaseAddress);
});

// MudBlazor
builder.Services.AddMudServices();

// Web infrastructure
builder.Services.AddWebSupport();

// Add authentication and authorization
builder.Services.AddAuthorizationCore();


// Add modules
builder.Services
    .AddCoreUiModule()
    .AddCatalogUiModule()
    .AddReportingUiModule()
    .AddIdentityUIModule();

var app = builder.Build();

// Initialize Identity state on startup
var stateManager = app.Services.GetRequiredService<WebSearchIndexing.Modules.Identity.Ui.State.IdentityStateManager>();
await stateManager.InitializeAsync();

await app.RunAsync();
