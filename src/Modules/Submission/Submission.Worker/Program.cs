using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using WebSearchIndexing.BuildingBlocks.Observability;
using WebSearchIndexing.Modules.Submission.Api;
using WebSearchIndexing.Modules.Submission.Application;
using WebSearchIndexing.Modules.Submission.Worker;

var builder = Host.CreateApplicationBuilder(args);

// Add Serilog
builder.Services.AddSerilog((serviceProvider, configuration) =>
    configuration.ReadFrom.Configuration(builder.Configuration)
        .WriteTo.Console());

// Add observability (OpenTelemetry)
builder.Services.AddObservability(builder.Configuration);

// Add modules
builder.Services.AddSubmissionModule();
builder.Services.AddSubmissionApplicationServices();

// Add the worker service
builder.Services.AddHostedService<SubmissionWorker>();

var host = builder.Build();

host.Run();
