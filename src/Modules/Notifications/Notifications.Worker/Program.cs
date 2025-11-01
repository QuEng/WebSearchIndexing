using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using WebSearchIndexing.Modules.Notifications.Api;
using WebSearchIndexing.Modules.Notifications.Worker;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddNotificationsModule();
builder.Services.AddHostedService<NotificationsWorker>();

var host = builder.Build();

host.Run();
