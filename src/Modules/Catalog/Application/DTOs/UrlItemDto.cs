using WebSearchIndexing.Modules.Catalog.Domain.Entities;

namespace WebSearchIndexing.Modules.Catalog.Application.DTOs;

public sealed record UrlItemDto(
    Guid Id,
    string Url,
    UrlItemType Type,
    UrlItemPriority Priority,
    UrlItemStatus Status,
    DateTime AddedAt,
    DateTime ProcessedAt,
    Guid? ServiceAccountId)
{
    public static UrlItemDto FromDomain(UrlItem urlItem)
    {
        ArgumentNullException.ThrowIfNull(urlItem);

        return new UrlItemDto(
            urlItem.Id,
            urlItem.Url,
            urlItem.Type,
            urlItem.Priority,
            urlItem.Status,
            urlItem.AddedAt,
            urlItem.ProcessedAt,
            urlItem.ServiceAccountId);
    }
}
