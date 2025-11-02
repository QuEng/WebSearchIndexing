using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using WebSearchIndexing.BuildingBlocks.Messaging.Outbox;

namespace WebSearchIndexing.BuildingBlocks.Messaging.BackgroundServices;

/// <summary>
/// Background service that processes outbox messages periodically
/// </summary>
public class OutboxBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<OutboxBackgroundService> _logger;
    private readonly TimeSpan _interval = TimeSpan.FromSeconds(30);

    public OutboxBackgroundService(
        IServiceProvider serviceProvider,
        ILogger<OutboxBackgroundService> logger)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Outbox background service started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var dispatcher = scope.ServiceProvider.GetRequiredService<IOutboxDispatcher>();

                await dispatcher.ProcessPendingMessagesAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while processing outbox messages");
            }

            await Task.Delay(_interval, stoppingToken);
        }

        _logger.LogInformation("Outbox background service stopped");
    }
}
