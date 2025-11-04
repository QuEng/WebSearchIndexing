using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using WebSearchIndexing.Modules.Inspection.Application.Abstractions;

namespace WebSearchIndexing.Modules.Inspection.Worker;

internal sealed class InspectionWorker : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<InspectionWorker> _logger;
    private readonly Meter _meter;
    private readonly Counter<int> _inspectedUrlsCounter;
    private readonly Counter<int> _failedInspectionsCounter;
    private readonly Counter<int> _retryRecommendationsCounter;
    private readonly Histogram<double> _inspectionDurationHistogram;

    private static readonly TimeSpan ProcessingInterval = TimeSpan.FromMinutes(3);

    public InspectionWorker(
        IServiceProvider serviceProvider,
        ILogger<InspectionWorker> logger)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        
        // Initialize metrics
        _meter = new Meter("WebSearchIndexing.Inspection.Worker", "1.0.0");
        _inspectedUrlsCounter = _meter.CreateCounter<int>("inspection_urls_inspected_total", 
            description: "Total number of URLs inspected by inspection worker");
        _failedInspectionsCounter = _meter.CreateCounter<int>("inspection_failures_total",
            description: "Total number of inspections that failed");
        _retryRecommendationsCounter = _meter.CreateCounter<int>("inspection_retry_recommendations_total",
            description: "Total number of retry recommendations made");
        _inspectionDurationHistogram = _meter.CreateHistogram<double>("inspection_duration_seconds",
            description: "Duration of inspection process in seconds");
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var _ = _logger.BeginScope("InspectionWorker");
        _logger.LogInformation("Inspection worker started");

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await ProcessInspectionsCycleAsync(stoppingToken);
                await Task.Delay(ProcessingInterval, stoppingToken);
            }
        }
        catch (OperationCanceledException)
        {
            // Graceful shutdown
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "Unhandled exception in inspection worker");
            throw;
        }

        _logger.LogInformation("Inspection worker stopping");
    }

    private async Task ProcessInspectionsCycleAsync(CancellationToken cancellationToken)
    {
        using var activity = Activity.Current?.Source.StartActivity("ProcessInspectionsCycle");
        var stopwatch = Stopwatch.StartNew();

        try
        {
            using var scope = _serviceProvider.CreateScope();
            var inspectionService = scope.ServiceProvider.GetRequiredService<IInspectionService>();

            _logger.LogDebug("Starting inspection processing cycle");

            // Check for pending inspections count first
            var pendingCount = await inspectionService.GetPendingInspectionCountAsync(cancellationToken);
            
            if (pendingCount > 0)
            {
                _logger.LogInformation("Processing {PendingCount} pending inspections", pendingCount);
                
                await inspectionService.ProcessPendingInspectionsAsync(cancellationToken);
                
                _inspectedUrlsCounter.Add(pendingCount, new TagList { { "status", "completed" } });
                _logger.LogInformation("Successfully processed {PendingCount} inspections", pendingCount);
            }
            else
            {
                _logger.LogDebug("No pending inspections found");
            }

            // Additional metrics for retry recommendations could be tracked here
            // when implementing the actual inspection logic
        }
        catch (Exception ex)
        {
            _failedInspectionsCounter.Add(1, new TagList { { "error", ex.GetType().Name } });
            _logger.LogError(ex, "Error occurred during inspection processing cycle");
        }
        finally
        {
            var duration = stopwatch.Elapsed.TotalSeconds;
            _inspectionDurationHistogram.Record(duration);
            
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
