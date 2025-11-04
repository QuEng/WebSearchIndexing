using Microsoft.Extensions.Logging;
using Polly;
using System.Net;
using WebSearchIndexing.BuildingBlocks.Messaging;
using WebSearchIndexing.Modules.Catalog.Domain.Entities;
using WebSearchIndexing.Modules.Crawler.Application.Abstractions;
using WebSearchIndexing.Modules.Crawler.Application.Events;

namespace WebSearchIndexing.Modules.Crawler.Application.Services;

/// <summary>
/// Implementation of crawler service for URL verification and preparation
/// </summary>
public sealed class CrawlerService : ICrawlerService
{
    private readonly IIntegrationEventPublisher _eventPublisher;
    private readonly HttpClient _httpClient;
    private readonly ILogger<CrawlerService> _logger;
    private readonly ResiliencePipeline _resiliencePipeline;

    public CrawlerService(
        IIntegrationEventPublisher eventPublisher,
        HttpClient httpClient,
        ILogger<CrawlerService> logger)
    {
        _eventPublisher = eventPublisher ?? throw new ArgumentNullException(nameof(eventPublisher));
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        // Configure resilience pipeline with exponential backoff for HTTP calls
        _resiliencePipeline = new ResiliencePipelineBuilder()
            .AddRetry(new Polly.Retry.RetryStrategyOptions
            {
                ShouldHandle = new PredicateBuilder()
                    .Handle<HttpRequestException>()
                    .Handle<TaskCanceledException>(),
                Delay = TimeSpan.FromSeconds(2),
                MaxRetryAttempts = 3,
                UseJitter = true,
            })
            .AddTimeout(TimeSpan.FromSeconds(10))
            .Build();
    }

    public async Task ProcessPendingUrlsAsync(CancellationToken cancellationToken = default)
    {
        using var scope = _logger.BeginScope("ProcessPendingUrls");
        _logger.LogInformation("Starting to process pending URLs for crawling");

        try
        {
            var pendingCount = await GetPendingUrlCountAsync(cancellationToken);
            
            if (pendingCount == 0)
            {
                _logger.LogDebug("No pending URLs found for crawling");
                return;
            }

            _logger.LogInformation("Found {PendingCount} URLs pending verification", pendingCount);

            // TODO: Query for actual pending URLs from database
            // For now, we'll simulate processing
            
            _logger.LogInformation("Completed processing pending URLs");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while processing pending URLs");
            throw;
        }
    }

    public async Task<bool> VerifyAndPrepareUrlAsync(UrlItem urlItem, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(urlItem);

        using var scope = _logger.BeginScope(new Dictionary<string, object>
        {
            ["UrlId"] = urlItem.Id,
            ["Url"] = urlItem.Url,
            ["Type"] = urlItem.Type
        });

        try
        {
            _logger.LogInformation("Verifying URL accessibility and content");

            // Verify URL accessibility using resilience pipeline
            var isAccessible = await _resiliencePipeline.ExecuteAsync(async (ct) =>
            {
                return await VerifyUrlAccessibilityAsync(urlItem.Url, ct);
            }, cancellationToken);

            if (!isAccessible)
            {
                _logger.LogWarning("URL is not accessible");
                
                // Publish event for failed verification
                var failureEvent = new UrlVerificationFailedIntegrationEvent(
                    urlItem.Id,
                    urlItem.Url,
                    "URL not accessible",
                    DateTime.UtcNow);

                await _eventPublisher.PublishAsync(failureEvent, cancellationToken);
                return false;
            }

            // Additional verification logic can be added here
            // e.g., content type validation, robots.txt checking, etc.

            _logger.LogInformation("URL verified successfully");

            // Publish event for successful verification
            var successEvent = new UrlVerificationSucceededIntegrationEvent(
                urlItem.Id,
                urlItem.Url,
                DateTime.UtcNow);

            await _eventPublisher.PublishAsync(successEvent, cancellationToken);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to verify URL");
            
            // Publish event for verification error
            var errorEvent = new UrlVerificationFailedIntegrationEvent(
                urlItem.Id,
                urlItem.Url,
                ex.Message,
                DateTime.UtcNow);

            await _eventPublisher.PublishAsync(errorEvent, cancellationToken);
            return false;
        }
    }

    public async Task<int> GetPendingUrlCountAsync(CancellationToken cancellationToken = default)
    {
        // TODO: Implement actual database query
        // For now, return a simulated count
        await Task.Delay(10, cancellationToken); // Simulate async work
        return 0; // No pending URLs for now
    }

    public async Task MarkUrlsReadyForSubmissionAsync(IEnumerable<UrlItem> urlItems, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(urlItems);

        var urlList = urlItems.ToList();
        if (urlList.Count == 0)
        {
            return;
        }

        _logger.LogInformation("Marking {Count} URLs as ready for submission", urlList.Count);

        try
        {
            // Publish events for URLs ready for submission
            foreach (var urlItem in urlList)
            {
                var readyEvent = new UrlReadyForSubmissionIntegrationEvent(
                    urlItem.Id,
                    urlItem.Url,
                    urlItem.Type,
                    DateTime.UtcNow);

                await _eventPublisher.PublishAsync(readyEvent, cancellationToken);
            }

            _logger.LogInformation("Successfully marked {Count} URLs as ready for submission", urlList.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to mark URLs as ready for submission");
            throw;
        }
    }

    private async Task<bool> VerifyUrlAccessibilityAsync(string url, CancellationToken cancellationToken)
    {
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Head, url);
            using var response = await _httpClient.SendAsync(request, cancellationToken);

            // Consider successful HTTP status codes
            var isSuccessful = response.StatusCode switch
            {
                HttpStatusCode.OK => true,
                HttpStatusCode.NoContent => true,
                HttpStatusCode.PartialContent => true,
                HttpStatusCode.Moved => true,
                HttpStatusCode.Redirect => true,
                HttpStatusCode.RedirectMethod => true,
                HttpStatusCode.NotModified => true,
                _ => false
            };

            _logger.LogDebug("URL accessibility check returned {StatusCode}", response.StatusCode);
            return isSuccessful;
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "URL accessibility check failed");
            return false;
        }
    }
}
