using WebSearchIndexing.BuildingBlocks.Messaging;

namespace WebSearchIndexing.Modules.Submission.Application.IntegrationEvents;

/// <summary>
/// Integration event raised when a URL batch is submitted for processing
/// </summary>
public sealed record BatchSubmittedIntegrationEvent : IIntegrationEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
    public string TenantId { get; init; } = string.Empty;
    public Guid BatchId { get; init; }
    public Guid SiteId { get; init; }
    public int ItemCount { get; init; }
    public DateTime SubmittedAt { get; init; }

    public BatchSubmittedIntegrationEvent(Guid batchId, Guid siteId, int itemCount, DateTime submittedAt, string tenantId = "")
    {
        BatchId = batchId;
        SiteId = siteId;
        ItemCount = itemCount;
        SubmittedAt = submittedAt;
        TenantId = tenantId;
    }
}

/// <summary>
/// Integration event raised when a batch validation fails
/// </summary>
public sealed record BatchValidationFailedIntegrationEvent : IIntegrationEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
    public string TenantId { get; init; } = string.Empty;
    public Guid BatchId { get; init; }
    public Guid SiteId { get; init; }
    public string Reason { get; init; } = string.Empty;
    public DateTime FailedAt { get; init; }

    public BatchValidationFailedIntegrationEvent(Guid batchId, Guid siteId, string reason, DateTime failedAt, string tenantId = "")
    {
        BatchId = batchId;
        SiteId = siteId;
        Reason = reason;
        FailedAt = failedAt;
        TenantId = tenantId;
    }
}

/// <summary>
/// Integration event raised when batch processing is completed
/// </summary>
public sealed record BatchProcessingCompletedIntegrationEvent : IIntegrationEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
    public string TenantId { get; init; } = string.Empty;
    public Guid BatchId { get; init; }
    public Guid SiteId { get; init; }
    public int ProcessedItemCount { get; init; }
    public int SuccessfulCount { get; init; }
    public int FailedCount { get; init; }
    public DateTime CompletedAt { get; init; }

    public BatchProcessingCompletedIntegrationEvent(
        Guid batchId, Guid siteId, int processedItemCount, int successfulCount, int failedCount, DateTime completedAt, string tenantId = "")
    {
        BatchId = batchId;
        SiteId = siteId;
        ProcessedItemCount = processedItemCount;
        SuccessfulCount = successfulCount;
        FailedCount = failedCount;
        CompletedAt = completedAt;
        TenantId = tenantId;
    }
}
