using WebSearchIndexing.BuildingBlocks.Messaging;

namespace WebSearchIndexing.Modules.Inspection.Application.IntegrationEvents;

/// <summary>
/// Integration event raised when URL inspection succeeds
/// </summary>
public sealed record UrlInspectionSucceededIntegrationEvent : IIntegrationEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
    public string TenantId { get; init; } = string.Empty;
    public Guid UrlId { get; init; }
    public string Url { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public DateTime InspectedAt { get; init; }

    public UrlInspectionSucceededIntegrationEvent(Guid urlId, string url, string status, DateTime inspectedAt, string tenantId = "")
    {
        UrlId = urlId;
        Url = url;
        Status = status;
        InspectedAt = inspectedAt;
        TenantId = tenantId;
    }
}

/// <summary>
/// Integration event raised when URL inspection fails
/// </summary>
public sealed record UrlInspectionFailedIntegrationEvent : IIntegrationEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
    public string TenantId { get; init; } = string.Empty;
    public Guid UrlId { get; init; }
    public string Url { get; init; } = string.Empty;
    public string ErrorMessage { get; init; } = string.Empty;
    public DateTime FailedAt { get; init; }

    public UrlInspectionFailedIntegrationEvent(Guid urlId, string url, string errorMessage, DateTime failedAt, string tenantId = "")
    {
        UrlId = urlId;
        Url = url;
        ErrorMessage = errorMessage;
        FailedAt = failedAt;
        TenantId = tenantId;
    }
}

/// <summary>
/// Integration event raised when a retry recommendation is made
/// </summary>
public sealed record RetryRecommendationIntegrationEvent : IIntegrationEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
    public string TenantId { get; init; } = string.Empty;
    public Guid UrlId { get; init; }
    public string Url { get; init; } = string.Empty;
    public bool ShouldRetry { get; init; }
    public TimeSpan DelayBeforeRetry { get; init; }
    public string Reason { get; init; } = string.Empty;
    public DateTime RecommendedAt { get; init; }

    public RetryRecommendationIntegrationEvent(
        Guid urlId, string url, bool shouldRetry, TimeSpan delayBeforeRetry, string reason, DateTime recommendedAt, string tenantId = "")
    {
        UrlId = urlId;
        Url = url;
        ShouldRetry = shouldRetry;
        DelayBeforeRetry = delayBeforeRetry;
        Reason = reason;
        RecommendedAt = recommendedAt;
        TenantId = tenantId;
    }
}
