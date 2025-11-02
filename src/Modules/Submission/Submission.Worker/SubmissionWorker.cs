using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace WebSearchIndexing.Modules.Submission.Worker;

internal sealed class SubmissionWorker : BackgroundService
{
    private readonly ILogger<SubmissionWorker> _logger;

    public SubmissionWorker(ILogger<SubmissionWorker> logger)
    {
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var _ = _logger.BeginScope("SubmissionWorker");
        _logger.LogInformation("Submission worker started");

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Submission idle tick");
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }
        }
        catch (OperationCanceledException)
        {
            // graceful shutdown
        }

        _logger.LogInformation("Submission worker stopping");
    }
}
