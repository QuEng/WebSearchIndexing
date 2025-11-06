using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using WebSearchIndexing.BuildingBlocks.Observability;
using WebSearchIndexing.Modules.Inspection.Api;
using WebSearchIndexing.Modules.Inspection.Application;
using WebSearchIndexing.Modules.Inspection.Worker;

var builder = Host.CreateApplicationBuilder(args);

// Add Serilog
builder.Services.AddSerilog((serviceProvider, configuration) =>
    configuration.ReadFrom.Configuration(builder.Configuration)
        .WriteTo.Console());

// Add observability (OpenTelemetry)
builder.Services.AddObservability(builder.Configuration);

// Add modules
builder.Services.AddInspectionModule();
builder.Services.AddInspectionApplicationServices();

// Add the worker service
builder.Services.AddHostedService<InspectionWorker>();

var host = builder.Build();

host.Run();
