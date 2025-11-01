namespace WebSearchIndexing.Modules.Core.Application.BackgroundJobs;

public interface IScopedRequestSendingService
{
    Task DoWork(CancellationToken stoppingToken);
}
