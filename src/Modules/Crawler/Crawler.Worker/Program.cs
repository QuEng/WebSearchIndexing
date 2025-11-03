using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using WebSearchIndexing.BuildingBlocks.Observability;
using WebSearchIndexing.Modules.Crawler.Api;
using WebSearchIndexing.Modules.Crawler.Application;
using WebSearchIndexing.Modules.Crawler.Worker;

var builder = Host.CreateApplicationBuilder(args);

// Add Serilog
builder.Services.AddSerilog((serviceProvider, configuration) =>
    configuration.ReadFrom.Configuration(builder.Configuration)
        .WriteTo.Console());

// Add observability (OpenTelemetry)
builder.Services.AddObservability();

// Add modules
builder.Services.AddCrawlerModule();
builder.Services.AddCrawlerApplicationServices();

// Add the worker service
builder.Services.AddHostedService<CrawlerWorker>();

var host = builder.Build();

host.Run();
