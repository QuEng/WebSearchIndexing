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
    private static readonly TimeSpan PollInterval = TimeSpan.FromMinutes(15);
    private readonly IUrlRequestRepository _urlRequestRepository = urlRequestRepository;
    private readonly ISettingsRepository _settingsRepository = settingsRepository;
    private readonly IServiceAccountRepository _serviceAccountRepository = serviceAccountRepository;
    private readonly ILogger _logger = logger;

    public async Task DoWork(CancellationToken stoppingToken)
    {
        using var _ = _logger.BeginScope("RequestSenderLoop");

        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Request sending loop tick");

            try
            {
                var setting = await _settingsRepository.GetAsync(stoppingToken);
                if (!setting.IsEnabled)
                {
                    _logger.LogInformation("Service disabled. Exiting worker loop.");
                    return;
                }

                var requestCountSentToday = await _urlRequestRepository.GetRequestsCountAsync(TimeSpan.FromDays(1), requestStatus: UrlItemStatus.Completed, cancellationToken: stoppingToken);
                if (requestCountSentToday >= setting.RequestsPerDay)
                {
                    _logger.LogInformation("Daily limit reached: {Count}/{Limit}. Sleeping1h.", requestCountSentToday, setting.RequestsPerDay);
                    await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
                    continue;
                }

                var countRequests = Math.Min(MaxRequestsPerBatch, setting.RequestsPerDay - requestCountSentToday);
                _logger.LogInformation("Taking up to {BatchCount} pending requests", countRequests);

                var urlRequests = await _urlRequestRepository.TakeRequestsAsync(countRequests, requestStatus: UrlItemStatus.Pending, cancellationToken: stoppingToken);
                _logger.LogInformation("Fetched {FetchedCount} pending requests", urlRequests.Count);

                foreach (var urlRequest in urlRequests)
                {
                    var serviceAccount = await _serviceAccountRepository.GetWithAvailableLimitAsync();
                    if (serviceAccount is null)
                    {
                        _logger.LogWarning("No service account with remaining quota. Breaking batch");
                        break;
                    }

                    using var scope = _logger.BeginScope(new Dictionary<string, object>
                    {
                        ["Url"] = urlRequest.Url,
                        ["UrlId"] = urlRequest.Id,
                        ["Type"] = urlRequest.Type,
                        ["ServiceAccountId"] = serviceAccount.Id
                    });

                    try
                    {
                        _logger.LogInformation("Sending request to Google Indexing API");
                        if (RequestSender.SendSingleRequest(serviceAccount, urlRequest, stoppingToken))
                        {
                            urlRequest.MarkCompleted(serviceAccount);
                            _logger.LogInformation("Request completed");
                        }
                        else
                        {
                            urlRequest.MarkFailed(serviceAccount);
                            _logger.LogWarning("Request failed (no exception). Marked failed");
                        }

                        await _urlRequestRepository.UpdateAsync(urlRequest);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Exception while processing");
                        urlRequest.MarkFailed(serviceAccount);
                        await _urlRequestRepository.UpdateAsync(urlRequest);
                    }
                }
            }
            catch (TaskCanceledException)
            {
                _logger.LogInformation("Worker cancellation requested");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled error in request sending loop");
            }

            _logger.LogInformation("Sleeping for {Interval}", PollInterval);
            await Task.Delay(PollInterval, stoppingToken);
        }
    }
}
