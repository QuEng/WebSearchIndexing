using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using WebSearchIndexing.Modules.Inspection.Api;
using WebSearchIndexing.Modules.Inspection.Worker;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddInspectionModule();
builder.Services.AddHostedService<InspectionWorker>();

var host = builder.Build();

host.Run();
