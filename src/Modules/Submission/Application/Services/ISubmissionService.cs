using WebSearchIndexing.Modules.Catalog.Domain;

namespace WebSearchIndexing.Modules.Submission.Application.Services;

/// <summary>
/// Service for handling URL batch submissions
/// </summary>
public interface ISubmissionService
{
    /// <summary>
    /// Process pending URL batches
    /// </summary>
    Task ProcessPendingBatchesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Validate and queue a batch of URLs
    /// </summary>
    Task<bool> ValidateAndQueueBatchAsync(UrlBatch batch, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get the number of pending batches
    /// </summary>
    Task<int> GetPendingBatchCountAsync(CancellationToken cancellationToken = default);
}
