namespace WebSearchIndexing.BackgroundJobs;

public class RequestSenderWorker(IServiceProvider services, ILogger<ScopedRequestSendingService> logger) : BackgroundService, IDisposable
{
    public IServiceProvider Services { get; } = services;
    private readonly ILogger<ScopedRequestSendingService> _logger = logger;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Request sending service running.");

        await DoWork(stoppingToken);
    }

    private async Task DoWork(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Request sending service is working.");

        using var scope = Services.CreateScope();

        var scopedProcessingService = scope.ServiceProvider.GetRequiredService<IScopedRequestSendingService>();

        await scopedProcessingService.DoWork(stoppingToken);
    }

    public override async Task StopAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Request sending service is stopping.");

        await base.StopAsync(stoppingToken);
    }
}