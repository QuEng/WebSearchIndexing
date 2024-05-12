namespace WebSearchIndexing.BackgroundJobs;

public interface IScopedRequestSendingService
{
    Task DoWork(CancellationToken stoppingToken);
}