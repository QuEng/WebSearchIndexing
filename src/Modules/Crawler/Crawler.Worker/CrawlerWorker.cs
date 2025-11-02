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
        using var _ = _logger.BeginScope("CrawlerWorker");
        _logger.LogInformation("Crawler worker started");

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Crawler idle tick");
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }
        }
        catch (OperationCanceledException)
        {
            // graceful shutdown
        }

        _logger.LogInformation("Crawler worker stopping");
    }
}
