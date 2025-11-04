using System.Diagnostics;
using System.Diagnostics.Metrics;
using WebSearchIndexing.Modules.Crawler.Application.Abstractions;

namespace WebSearchIndexing.Modules.Crawler.Worker;

internal sealed class CrawlerWorker : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<CrawlerWorker> _logger;
    private readonly Meter _meter;
    private readonly Counter<int> _verifiedUrlsCounter;
    private readonly Counter<int> _failedVerificationsCounter;
    private readonly Histogram<double> _verificationDurationHistogram;

    private static readonly TimeSpan ProcessingInterval = TimeSpan.FromMinutes(2);

    public CrawlerWorker(
        IServiceProvider serviceProvider,
        ILogger<CrawlerWorker> logger)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        // Initialize metrics
        _meter = new Meter("WebSearchIndexing.Crawler.Worker", "1.0.0");
        _verifiedUrlsCounter = _meter.CreateCounter<int>("crawler_urls_verified_total",
            description: "Total number of URLs verified by crawler worker");
        _failedVerificationsCounter = _meter.CreateCounter<int>("crawler_verifications_failed_total",
            description: "Total number of URL verifications that failed");
        _verificationDurationHistogram = _meter.CreateHistogram<double>("crawler_verification_duration_seconds",
            description: "Duration of URL verification process in seconds");
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var _ = _logger.BeginScope("CrawlerWorker");
        _logger.LogInformation("Crawler worker started");

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await ProcessUrlsCycleAsync(stoppingToken);
                await Task.Delay(ProcessingInterval, stoppingToken);
            }
        }
        catch (OperationCanceledException)
        {
            // Graceful shutdown
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "Unhandled exception in crawler worker");
            throw;
        }

        _logger.LogInformation("Crawler worker stopping");
    }

    private async Task ProcessUrlsCycleAsync(CancellationToken cancellationToken)
    {
        using var activity = Activity.Current?.Source.StartActivity("ProcessUrlsCycle");
        var stopwatch = Stopwatch.StartNew();

        try
        {
            using var scope = _serviceProvider.CreateScope();
            var crawlerService = scope.ServiceProvider.GetRequiredService<ICrawlerService>();

            _logger.LogDebug("Starting URL processing cycle");

            // Check for pending URLs count first
            var pendingCount = await crawlerService.GetPendingUrlCountAsync(cancellationToken);

            if (pendingCount > 0)
            {
                _logger.LogInformation("Processing {PendingCount} pending URLs for verification", pendingCount);

                await crawlerService.ProcessPendingUrlsAsync(cancellationToken);

                _verifiedUrlsCounter.Add(pendingCount, new TagList { { "status", "processed" } });
                _logger.LogInformation("Successfully processed {PendingCount} URLs", pendingCount);
            }
            else
            {
                _logger.LogDebug("No pending URLs found for verification");
            }
        }
        catch (Exception ex)
        {
            _failedVerificationsCounter.Add(1, new TagList { { "error", ex.GetType().Name } });
            _logger.LogError(ex, "Error occurred during URL processing cycle");
        }
        finally
        {
            var duration = stopwatch.Elapsed.TotalSeconds;
            _verificationDurationHistogram.Record(duration);

            if (activity != null)
            {
                activity.SetTag("duration_seconds", duration);
            }
        }
    }

    public override void Dispose()
    {
        _meter?.Dispose();
        base.Dispose();
    }
}
