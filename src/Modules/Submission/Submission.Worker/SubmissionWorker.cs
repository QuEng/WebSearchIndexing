using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using WebSearchIndexing.Modules.Submission.Application.Abstractions;

namespace WebSearchIndexing.Modules.Submission.Worker;

internal sealed class SubmissionWorker : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<SubmissionWorker> _logger;
    private readonly Meter _meter;
    private readonly Counter<int> _processedBatchesCounter;
    private readonly Counter<int> _failedBatchesCounter;
    private readonly Histogram<double> _processingDurationHistogram;

    private static readonly TimeSpan ProcessingInterval = TimeSpan.FromMinutes(1);

    public SubmissionWorker(
        IServiceProvider serviceProvider,
        ILogger<SubmissionWorker> logger)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        
        // Initialize metrics
        _meter = new Meter("WebSearchIndexing.Submission.Worker", "1.0.0");
        _processedBatchesCounter = _meter.CreateCounter<int>("submission_batches_processed_total", 
            description: "Total number of batches processed by submission worker");
        _failedBatchesCounter = _meter.CreateCounter<int>("submission_batches_failed_total",
            description: "Total number of batches that failed processing");
        _processingDurationHistogram = _meter.CreateHistogram<double>("submission_processing_duration_seconds",
            description: "Duration of batch processing in seconds");
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var _ = _logger.BeginScope("SubmissionWorker");
        _logger.LogInformation("Submission worker started");

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await ProcessBatchesCycleAsync(stoppingToken);
                await Task.Delay(ProcessingInterval, stoppingToken);
            }
        }
        catch (OperationCanceledException)
        {
            // Graceful shutdown
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "Unhandled exception in submission worker");
            throw;
        }

        _logger.LogInformation("Submission worker stopping");
    }

    private async Task ProcessBatchesCycleAsync(CancellationToken cancellationToken)
    {
        using var activity = Activity.Current?.Source.StartActivity("ProcessBatchesCycle");
        var stopwatch = Stopwatch.StartNew();

        try
        {
            using var scope = _serviceProvider.CreateScope();
            var submissionService = scope.ServiceProvider.GetRequiredService<ISubmissionService>();

            _logger.LogDebug("Starting batch processing cycle");

            // Check for pending batches count first
            var pendingCount = await submissionService.GetPendingBatchCountAsync(cancellationToken);
            
            if (pendingCount > 0)
            {
                _logger.LogInformation("Processing {PendingCount} pending batches", pendingCount);
                
                await submissionService.ProcessPendingBatchesAsync(cancellationToken);
                
                _processedBatchesCounter.Add(pendingCount, new TagList { { "status", "completed" } });
                _logger.LogInformation("Successfully processed {PendingCount} batches", pendingCount);
            }
            else
            {
                _logger.LogDebug("No pending batches found");
            }
        }
        catch (Exception ex)
        {
            _failedBatchesCounter.Add(1, new TagList { { "error", ex.GetType().Name } });
            _logger.LogError(ex, "Error occurred during batch processing cycle");
        }
        finally
        {
            var duration = stopwatch.Elapsed.TotalSeconds;
            _processingDurationHistogram.Record(duration);
            
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
