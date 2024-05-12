using WebSearchIndexing.Domain.Repositories;

namespace WebSearchIndexing.BackgroundJobs;

public class ScopedRequestSendingService(IUrlRequestRepository urlRequestRepository,
                                         ISettingRepository settingRepository,
                                         IServiceAccountRepository serviceAccountRepository,
                                         ILogger<ScopedRequestSendingService> logger) : IScopedRequestSendingService
{
    private const int MAX_REQUESTS_PER_TIME_COUNT = 100;
    private readonly IUrlRequestRepository _urlRequestRepository = urlRequestRepository;
    private readonly ISettingRepository _settingRepository = settingRepository;
    private readonly IServiceAccountRepository _serviceAccountRepository = serviceAccountRepository;
    private readonly ILogger _logger = logger;

    public async Task DoWork(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Request sending service is working");

            try
            {
                var setting = await _settingRepository.GetSettingAsync();
                if (!setting.IsEnabled)
                {
                    _logger.LogInformation("Service is disabled. Service will stop until it is enabled in the settings.");
                    return;
                }
                var maxRequestCount = setting.RequestsPerDay;
                var requestCountSendedToday = await _urlRequestRepository.GetRequestsCountAsync(TimeSpan.FromDays(1), requestStatus: Domain.Entities.UrlRequestStatus.Completed, cancellationToken: stoppingToken);

                if (requestCountSendedToday >= maxRequestCount)
                {
                    _logger.LogInformation("Max request limit reached. Service will pause for an hour.");
                    await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
                    continue;
                }

                int countRequests = MAX_REQUESTS_PER_TIME_COUNT;
                if ((maxRequestCount - requestCountSendedToday) <= MAX_REQUESTS_PER_TIME_COUNT)
                {
                    countRequests = maxRequestCount - requestCountSendedToday;
                }

                var urlRequests = await _urlRequestRepository.TakeRequestsAsync(countRequests, requestStatus: Domain.Entities.UrlRequestStatus.Pending, cancellationToken: stoppingToken);

                foreach (var urlRequest in urlRequests)
                {
                    var serviceAccount = await _serviceAccountRepository.GetWithAvailableLimitAsync();

                    try
                    {
                        if (RequestSender.SendSingleRequest(serviceAccount, urlRequest, stoppingToken))
                        {
                            urlRequest.Status = Domain.Entities.UrlRequestStatus.Completed;
                        }
                        else
                        {
                            urlRequest.Status = Domain.Entities.UrlRequestStatus.Failed;
                        }
                        urlRequest.ProcessedAt = DateTime.UtcNow;
                        urlRequest.ServiceAccountId = serviceAccount.Id;
                        await _urlRequestRepository.UpdateAsync(urlRequest);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Exception");
                    }

                }
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogError(ex, "Task canceled exception");
            }

            await Task.Delay(TimeSpan.FromMinutes(15), stoppingToken);
        }
    }
}