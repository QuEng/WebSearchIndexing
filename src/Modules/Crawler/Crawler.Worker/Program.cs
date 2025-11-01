using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using WebSearchIndexing.Modules.Crawler.Api;
using WebSearchIndexing.Modules.Crawler.Worker;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddCrawlerModule();
builder.Services.AddHostedService<CrawlerWorker>();

var host = builder.Build();

host.Run();
