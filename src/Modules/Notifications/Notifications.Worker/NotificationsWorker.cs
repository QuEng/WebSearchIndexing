using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace WebSearchIndexing.Modules.Notifications.Worker;

internal sealed class NotificationsWorker : BackgroundService
{
    private readonly ILogger<NotificationsWorker> _logger;

    public NotificationsWorker(ILogger<NotificationsWorker> logger)
    {
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Notifications worker started.");

        try
        {
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (OperationCanceledException)
        {
            // graceful shutdown
        }

        _logger.LogInformation("Notifications worker stopping.");
    }
}
