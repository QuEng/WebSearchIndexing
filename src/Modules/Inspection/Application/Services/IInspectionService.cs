using WebSearchIndexing.Modules.Catalog.Domain;

namespace WebSearchIndexing.Modules.Inspection.Application.Services;

/// <summary>
/// Service for inspecting URL status and analyzing errors
/// </summary>
public interface IInspectionService
{
    /// <summary>
    /// Process URLs that need status validation and retry analysis
    /// </summary>
    Task ProcessPendingInspectionsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Inspect a URL item's status and determine next action
    /// </summary>
    Task<InspectionResult> InspectUrlStatusAsync(UrlItem urlItem, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get the number of URLs pending inspection
    /// </summary>
    Task<int> GetPendingInspectionCountAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Analyze failed URLs and determine retry strategy
    /// </summary>
    Task<RetryRecommendation> AnalyzeFailureAsync(UrlItem urlItem, CancellationToken cancellationToken = default);
}

/// <summary>
/// Result of URL inspection
/// </summary>
public sealed record InspectionResult(
    bool IsSuccessful,
    string Status,
    string? ErrorMessage = null,
    DateTime InspectedAt = default);

/// <summary>
/// Recommendation for retry strategy
/// </summary>
public sealed record RetryRecommendation(
    bool ShouldRetry,
    TimeSpan DelayBeforeRetry,
    string Reason);
