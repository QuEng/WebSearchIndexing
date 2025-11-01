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
        _logger.LogInformation("Submission worker started.");

        try
        {
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (OperationCanceledException)
        {
            // graceful shutdown
        }

        _logger.LogInformation("Submission worker stopping.");
    }
}
