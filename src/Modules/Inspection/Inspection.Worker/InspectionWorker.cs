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
        using var _ = _logger.BeginScope("InspectionWorker");
        _logger.LogInformation("Inspection worker started");

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Inspection idle tick");
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }
        }
        catch (OperationCanceledException)
        {
            // graceful shutdown
        }

        _logger.LogInformation("Inspection worker stopping");
    }
}
