namespace WebSearchIndexing.Domain.Entities;

public class UrlRequest : BaseEntity<Guid>
{
    public UrlRequest()
    {
        Id = Guid.NewGuid();
    }

    public string Url { get; set; } = string.Empty;
    public UrlRequestType Type { get; set; }
    public UrlRequstPriority Priority { get; set; }
    public UrlRequestStatus Status { get; set; }
    public DateTime AddedAt { get; set; }
    public DateTime ProcessedAt { get; set; }
    public Guid? ServiceAccountId { get; set; }
    public virtual ServiceAccount? ServiceAccount { get; set; } = null!;
}

public enum UrlRequestType
{
    Updated,
    Deleted
}

public enum UrlRequstPriority
{
    Low,
    Medium,
    High
}

public enum UrlRequestStatus
{
    Pending,
    Completed,
    Failed
}