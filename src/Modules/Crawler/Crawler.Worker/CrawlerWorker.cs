using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace WebSearchIndexing.Modules.Crawler.Worker;

internal sealed class CrawlerWorker : BackgroundService
{
    private readonly ILogger<CrawlerWorker> _logger;

    public CrawlerWorker(ILogger<CrawlerWorker> logger)
    {
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Crawler worker started.");

        try
        {
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (OperationCanceledException)
        {
            // graceful shutdown
        }

        _logger.LogInformation("Crawler worker stopping.");
    }
}
