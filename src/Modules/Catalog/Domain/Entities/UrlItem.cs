using WebSearchIndexing.BuildingBlocks.Abstractions.Entities;

namespace WebSearchIndexing.Modules.Catalog.Domain.Entities;

public sealed class UrlItem : IEntity<Guid>
{
    private UrlItem()
    {
        // For EF
    }

    public UrlItem(string url, UrlItemType type, UrlItemPriority priority)
    {
        Id = Guid.NewGuid();
        UpdateUrl(url);
        Type = type;
        Priority = priority;
        Status = UrlItemStatus.Pending;
        AddedAt = DateTime.UtcNow;
        ProcessedAt = DateTime.MinValue;
    }

    public Guid Id { get; private set; }
    public string Url { get; private set; } = string.Empty;
    public UrlItemType Type { get; private set; }
    public UrlItemPriority Priority { get; private set; }
    public UrlItemStatus Status { get; private set; }
    public DateTime AddedAt { get; private set; }
    public DateTime ProcessedAt { get; private set; }
    public Guid? ServiceAccountId { get; private set; }
    public ServiceAccount? ServiceAccount { get; private set; }
    public int FailureCount { get; private set; }

    public bool IsPending => Status == UrlItemStatus.Pending;
    public bool IsCompleted => Status == UrlItemStatus.Completed;
    public bool IsFailed => Status == UrlItemStatus.Failed;

    public void UpdateUrl(string url)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(url);
        Url = url.Trim();
    }

    public void UpdatePriority(UrlItemPriority priority) => Priority = priority;

    public void UpdateType(UrlItemType type) => Type = type;

    public void AssignTo(ServiceAccount account)
    {
        ServiceAccount = account ?? throw new ArgumentNullException(nameof(account));
        ServiceAccountId = account.Id;
    }

    public void MarkCompleted(ServiceAccount account)
    {
        AssignTo(account);
        Status = UrlItemStatus.Completed;
        ProcessedAt = DateTime.UtcNow;
    }

    public void MarkFailed(ServiceAccount account)
    {
        AssignTo(account);
        Status = UrlItemStatus.Failed;
        ProcessedAt = DateTime.UtcNow;
        FailureCount++;
    }

    public void MarkPending()
    {
        Status = UrlItemStatus.Pending;
        ProcessedAt = DateTime.MinValue;
        ServiceAccountId = null;
        ServiceAccount = null;
        // Don't reset FailureCount when marking as pending - keep the history
    }
}

public enum UrlItemType
{
    Updated = 0,
    Deleted = 1
}

public enum UrlItemPriority
{
    Low = 0,
    Medium = 1,
    High = 2
}

public enum UrlItemStatus
{
    Pending = 0,
    Completed = 1,
    Failed = 2
}
