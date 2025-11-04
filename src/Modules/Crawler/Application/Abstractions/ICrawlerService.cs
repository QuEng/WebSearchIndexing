using WebSearchIndexing.Modules.Catalog.Domain.Entities;

namespace WebSearchIndexing.Modules.Crawler.Application.Abstractions;

/// <summary>
/// Service for crawling and verifying URLs
/// </summary>
public interface ICrawlerService
{
    /// <summary>
    /// Process URLs that need to be crawled and verified
    /// </summary>
    Task ProcessPendingUrlsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Verify and prepare a URL for indexing
    /// </summary>
    Task<bool> VerifyAndPrepareUrlAsync(UrlItem urlItem, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get the number of URLs pending verification
    /// </summary>
    Task<int> GetPendingUrlCountAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Mark URLs as ready for submission to indexing service
    /// </summary>
    Task MarkUrlsReadyForSubmissionAsync(IEnumerable<UrlItem> urlItems, CancellationToken cancellationToken = default);
}
