using WebSearchIndexing.Modules.Catalog.Application.DTOs;
using WebSearchIndexing.Modules.Catalog.Domain;

namespace WebSearchIndexing.Modules.Catalog.Ui.Pages.Urls.Components;

public sealed class EditableUrlItem
{
    public EditableUrlItem(UrlItemDto dto)
    {
        Id = dto.Id;
        Url = dto.Url;
        Type = dto.Type;
        Priority = dto.Priority;
        Status = dto.Status;
        AddedAt = dto.AddedAt;
        ProcessedAt = dto.ProcessedAt;
        ServiceAccountId = dto.ServiceAccountId;
    }

    public Guid Id { get; }
    public string Url { get; set; }
    public UrlItemType Type { get; }
    public UrlItemPriority Priority { get; set; }
    public UrlItemStatus Status { get; }
    public DateTime AddedAt { get; }
    public DateTime ProcessedAt { get; }
    public Guid? ServiceAccountId { get; }

    public bool IsPending => Status == UrlItemStatus.Pending;
    public bool IsCompleted => Status == UrlItemStatus.Completed;
    public bool IsFailed => Status == UrlItemStatus.Failed;

    public void UpdateUrl(string url)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(url);
        Url = url.Trim();
    }

    public void UpdatePriority(UrlItemPriority priority) => Priority = priority;

    public UrlItemDto ToDto() => new(Id, Url, Type, Priority, Status, AddedAt, ProcessedAt, ServiceAccountId);
}
