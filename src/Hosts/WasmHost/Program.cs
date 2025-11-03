using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;
using WebSearchIndexing.Hosts.WasmHost;
using WebSearchIndexing.Modules.Core.Ui;
using WebSearchIndexing.Modules.Catalog.Ui;
using WebSearchIndexing.Modules.Reporting.Ui;
using WebSearchIndexing.BuildingBlocks.Web;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddHttpClient("WebSearchIndexing.Api", client => 
{
    client.BaseAddress = new Uri("http://localhost:5093/"); // Point to our ApiHost
});

builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("WebSearchIndexing.Api"));

// MudBlazor
builder.Services.AddMudServices();

// Web infrastructure
builder.Services.AddWebSupport();

builder.Services
    .AddCoreUiModule()
    .AddCatalogUiModule()
    .AddReportingUiModule();

await builder.Build().RunAsync();
