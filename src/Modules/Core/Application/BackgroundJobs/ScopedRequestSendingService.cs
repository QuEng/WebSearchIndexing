using Microsoft.Extensions.Logging;
using WebSearchIndexing.Modules.Catalog.Application.Abstractions;
using WebSearchIndexing.Modules.Catalog.Domain;
using WebSearchIndexing.Modules.Core.Application;
using WebSearchIndexing.Modules.Core.Domain;

namespace WebSearchIndexing.Modules.Core.Application.BackgroundJobs;

public sealed class ScopedRequestSendingService(
    IUrlRequestRepository urlRequestRepository,
    ISettingsRepository settingsRepository,
    IServiceAccountRepository serviceAccountRepository,
    ILogger<ScopedRequestSendingService> logger) : IScopedRequestSendingService
{
    private const int MaxRequestsPerBatch = 100;
    private readonly IUrlRequestRepository _urlRequestRepository = urlRequestRepository;
    private readonly ISettingsRepository _settingsRepository = settingsRepository;
    private readonly IServiceAccountRepository _serviceAccountRepository = serviceAccountRepository;
    private readonly ILogger _logger = logger;

    public async Task DoWork(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Request sending service is working");

            try
            {
                var setting = await _settingsRepository.GetAsync(stoppingToken);
                if (!setting.IsEnabled)
                {
                    _logger.LogInformation("Service is disabled. Service will stop until it is enabled in the settings.");
                    return;
                }

                var requestCountSentToday = await _urlRequestRepository.GetRequestsCountAsync(TimeSpan.FromDays(1), requestStatus: UrlItemStatus.Completed, cancellationToken: stoppingToken);
                if (requestCountSentToday >= setting.RequestsPerDay)
                {
                    _logger.LogInformation("Max request limit reached. Service will pause for an hour.");
                    await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
                    continue;
                }

                var countRequests = Math.Min(MaxRequestsPerBatch, setting.RequestsPerDay - requestCountSentToday);

                var urlRequests = await _urlRequestRepository.TakeRequestsAsync(countRequests, requestStatus: UrlItemStatus.Pending, cancellationToken: stoppingToken);

                foreach (var urlRequest in urlRequests)
                {
                    var serviceAccount = await _serviceAccountRepository.GetWithAvailableLimitAsync();
                    if (serviceAccount is null)
                    {
                        _logger.LogWarning("No service account available with remaining quota.");
                        break;
                    }

                    try
                    {
                        if (RequestSender.SendSingleRequest(serviceAccount, urlRequest, stoppingToken))
                        {
                            urlRequest.MarkCompleted(serviceAccount);
                        }
                        else
                        {
                            urlRequest.MarkFailed(serviceAccount);
                        }

                        await _urlRequestRepository.UpdateAsync(urlRequest);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Exception occurred while processing URL {Url}", urlRequest.Url);
                        urlRequest.MarkFailed(serviceAccount);
                        await _urlRequestRepository.UpdateAsync(urlRequest);
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
