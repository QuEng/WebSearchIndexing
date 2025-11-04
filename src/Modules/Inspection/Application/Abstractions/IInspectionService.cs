using WebSearchIndexing.Modules.Catalog.Domain.Entities;
using WebSearchIndexing.Modules.Inspection.Application.Services;

namespace WebSearchIndexing.Modules.Inspection.Application.Abstractions;

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
