using WebSearchIndexing.BuildingBlocks.Messaging;
using WebSearchIndexing.Modules.Catalog.Domain.Entities;

namespace WebSearchIndexing.Modules.Crawler.Application.Events;

/// <summary>
/// Integration event raised when URL verification succeeds
/// </summary>
public sealed record UrlVerificationSucceededIntegrationEvent : IIntegrationEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
    public string TenantId { get; init; } = string.Empty;
    public Guid UrlId { get; init; }
    public string Url { get; init; } = string.Empty;
    public DateTime VerifiedAt { get; init; }

    public UrlVerificationSucceededIntegrationEvent(Guid urlId, string url, DateTime verifiedAt, string tenantId = "")
    {
        UrlId = urlId;
        Url = url;
        VerifiedAt = verifiedAt;
        TenantId = tenantId;
    }
}

/// <summary>
/// Integration event raised when URL verification fails
/// </summary>
public sealed record UrlVerificationFailedIntegrationEvent : IIntegrationEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
    public string TenantId { get; init; } = string.Empty;
    public Guid UrlId { get; init; }
    public string Url { get; init; } = string.Empty;
    public string Reason { get; init; } = string.Empty;
    public DateTime FailedAt { get; init; }

    public UrlVerificationFailedIntegrationEvent(Guid urlId, string url, string reason, DateTime failedAt, string tenantId = "")
    {
        UrlId = urlId;
        Url = url;
        Reason = reason;
        FailedAt = failedAt;
        TenantId = tenantId;
    }
}

/// <summary>
/// Integration event raised when URL is ready for submission to indexing service
/// </summary>
public sealed record UrlReadyForSubmissionIntegrationEvent : IIntegrationEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
    public string TenantId { get; init; } = string.Empty;
    public Guid UrlId { get; init; }
    public string Url { get; init; } = string.Empty;
    public UrlItemType UrlType { get; init; }
    public DateTime ReadyAt { get; init; }

    public UrlReadyForSubmissionIntegrationEvent(Guid urlId, string url, UrlItemType urlType, DateTime readyAt, string tenantId = "")
    {
        UrlId = urlId;
        Url = url;
        UrlType = urlType;
        ReadyAt = readyAt;
        TenantId = tenantId;
    }
}
