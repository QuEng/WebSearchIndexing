namespace WebSearchIndexing.Modules.Inspection.Application.Services;

/// <summary>
/// Result of URL inspection
/// </summary>
public sealed record InspectionResult(
    bool IsSuccessful,
    string Status,
    string? ErrorMessage,
    DateTime InspectedAt);

/// <summary>
/// Recommendation for retry strategy
/// </summary>
public sealed record RetryRecommendation(
    bool ShouldRetry,
    TimeSpan DelayBeforeRetry,
    string Reason);
