using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace WebSearchIndexing.Modules.Inspection.Worker;

internal sealed class InspectionWorker : BackgroundService
{
    private readonly ILogger<InspectionWorker> _logger;

    public InspectionWorker(ILogger<InspectionWorker> logger)
    {
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Inspection worker started.");

        try
        {
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (OperationCanceledException)
        {
            // graceful shutdown
        }

        _logger.LogInformation("Inspection worker stopping.");
    }
}
