using WebSearchIndexing.BuildingBlocks.Messaging;

namespace WebSearchIndexing.Modules.Identity.Application.IntegrationEvents.External;

// Copy of Catalog Integration Events to avoid direct module dependencies
// These should be kept in sync with the original events in Catalog module

public class ServiceAccountCreatedEvent : IntegrationEvent
{
    public Guid ServiceAccountId { get; }
    public string ProjectId { get; }
    public uint QuotaLimitPerDay { get; }

    public ServiceAccountCreatedEvent(string tenantId, Guid serviceAccountId, string projectId, uint quotaLimitPerDay) 
        : base(tenantId)
    {
        ServiceAccountId = serviceAccountId;
        ProjectId = projectId ?? throw new ArgumentNullException(nameof(projectId));
        QuotaLimitPerDay = quotaLimitPerDay;
    }
}

public class ServiceAccountUpdatedEvent : IntegrationEvent
{
    public Guid ServiceAccountId { get; }
    public string ProjectId { get; }
    public uint QuotaLimitPerDay { get; }

    public ServiceAccountUpdatedEvent(string tenantId, Guid serviceAccountId, string projectId, uint quotaLimitPerDay) 
        : base(tenantId)
    {
        ServiceAccountId = serviceAccountId;
        ProjectId = projectId ?? throw new ArgumentNullException(nameof(projectId));
        QuotaLimitPerDay = quotaLimitPerDay;
    }
}

public class ServiceAccountDeletedEvent : IntegrationEvent
{
    public Guid ServiceAccountId { get; }

    public ServiceAccountDeletedEvent(string tenantId, Guid serviceAccountId) 
        : base(tenantId)
    {
        ServiceAccountId = serviceAccountId;
    }
}

public class UrlItemsImportedEvent : IntegrationEvent
{
    public Guid BatchId { get; }
    public int UrlCount { get; }
    public string[] UrlPatterns { get; }

    public UrlItemsImportedEvent(string tenantId, Guid batchId, int urlCount, string[] urlPatterns) 
        : base(tenantId)
    {
        BatchId = batchId;
        UrlCount = urlCount;
        UrlPatterns = urlPatterns ?? throw new ArgumentNullException(nameof(urlPatterns));
    }
}

public class UrlItemStatusChangedEvent : IntegrationEvent
{
    public Guid UrlItemId { get; }
    public string OldStatus { get; }
    public string NewStatus { get; }
    public string? ErrorMessage { get; }

    public UrlItemStatusChangedEvent(string tenantId, Guid urlItemId, string oldStatus, string newStatus, string? errorMessage = null) 
        : base(tenantId)
    {
        UrlItemId = urlItemId;
        OldStatus = oldStatus ?? throw new ArgumentNullException(nameof(oldStatus));
        NewStatus = newStatus ?? throw new ArgumentNullException(nameof(newStatus));
        ErrorMessage = errorMessage;
    }
}
