using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using WebSearchIndexing.Modules.Submission.Api;
using WebSearchIndexing.Modules.Submission.Worker;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddSubmissionModule();
builder.Services.AddHostedService<SubmissionWorker>();

var host = builder.Build();

host.Run();
