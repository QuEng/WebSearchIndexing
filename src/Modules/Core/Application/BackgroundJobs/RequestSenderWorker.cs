using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace WebSearchIndexing.Modules.Core.Application.BackgroundJobs;

public sealed class RequestSenderWorker(IServiceProvider services, ILogger<RequestSenderWorker> logger) : BackgroundService
{
    public IServiceProvider Services { get; } = services;
    private readonly ILogger<RequestSenderWorker> _logger = logger;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var _ = _logger.BeginScope("RequestSenderWorker");
        _logger.LogInformation("Request sending worker started");

        await DoWork(stoppingToken);

        _logger.LogInformation("Request sending worker finished");
    }

    private async Task DoWork(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Creating scope for scoped processing service");

        using var scope = Services.CreateScope();

        var scopedProcessingService = scope.ServiceProvider.GetRequiredService<IScopedRequestSendingService>();

        await scopedProcessingService.DoWork(stoppingToken);
    }

    public override async Task StopAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Request sending worker stopping...");

        await base.StopAsync(stoppingToken);

        _logger.LogInformation("Request sending worker stopped");
    }
}
