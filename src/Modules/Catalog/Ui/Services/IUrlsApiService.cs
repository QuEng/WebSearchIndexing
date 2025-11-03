using WebSearchIndexing.Modules.Catalog.Application.DTOs;
using WebSearchIndexing.Modules.Catalog.Domain;

namespace WebSearchIndexing.Modules.Catalog.Ui.Services;

public interface IUrlsApiService
{
    Task<IReadOnlyCollection<UrlItemDto>> GetUrlsAsync(
        int count = 10,
        int offset = 0,
        UrlItemStatus? status = null,
        UrlItemType? type = null,
        Guid? serviceAccountId = null,
        TimeSpan? subtractTime = null,
        CancellationToken cancellationToken = default);

    Task<int> GetUrlsCountAsync(
        UrlItemStatus? status = null,
        UrlItemType? type = null,
        Guid? serviceAccountId = null,
        TimeSpan? subtractTime = null,
        CancellationToken cancellationToken = default);

    Task<UrlItemDto> UpdateUrlAsync(
        Guid id,
        string url,
        UrlItemPriority priority,
        CancellationToken cancellationToken = default);

    Task<bool> DeleteUrlAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<bool> DeleteUrlsBatchAsync(
        IReadOnlyCollection<Guid> ids,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<UrlItemDto>> ImportUrlsAsync(
        IReadOnlyCollection<ImportUrlEntry> urls,
        Guid? siteId = null,
        CancellationToken cancellationToken = default);
}

public sealed record ImportUrlEntry(
    string Url,
    UrlItemType Type,
    UrlItemPriority Priority);
