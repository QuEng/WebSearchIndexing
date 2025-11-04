using WebSearchIndexing.Modules.Catalog.Domain.Entities;

namespace WebSearchIndexing.Modules.Catalog.Application.Abstractions;

public interface IUrlRequestRepository
{
    Task<UrlItem> AddAsync(UrlItem entity, CancellationToken cancellationToken = default);

    Task<bool> UpdateAsync(UrlItem entity, CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);

    Task<UrlItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<List<UrlItem>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<bool> AddRangeAsync(IEnumerable<UrlItem> urlRequests, CancellationToken cancellationToken = default);

    Task<bool> RemoveRangeAsync(IEnumerable<UrlItem> urlRequests, CancellationToken cancellationToken = default);

    Task<int> GetRequestsCountAsync(
        UrlItemStatus? requestStatus = default,
        UrlItemType? requestType = default,
        CancellationToken cancellationToken = default);

    Task<int> GetRequestsCountAsync(
        Guid serviceAccountId,
        UrlItemStatus? requestStatus = default,
        UrlItemType? requestType = default,
        CancellationToken cancellationToken = default);

    Task<int> GetRequestsCountAsync(
        TimeSpan subtractTime,
        UrlItemStatus? requestStatus = default,
        UrlItemType? requestType = default,
        CancellationToken cancellationToken = default);

    Task<int> GetRequestsCountAsync(
        TimeSpan subtractTime,
        Guid serviceAccountId,
        UrlItemStatus? requestStatus = default,
        UrlItemType? requestType = default,
        CancellationToken cancellationToken = default);

    Task<List<UrlItem>> TakeRequestsAsync(
        int count,
        int? offset = 0,
        UrlItemStatus? requestStatus = default,
        UrlItemType? requestType = default,
        CancellationToken cancellationToken = default);

    Task<List<UrlItem>> TakeRequestsAsync(
        int count,
        Guid serviceAccountId,
        int? offset = 0,
        UrlItemStatus? requestStatus = default,
        UrlItemType? requestType = default,
        CancellationToken cancellationToken = default);

    Task<List<UrlItem>> TakeRequestsAsync(
        int count,
        TimeSpan subtractTime,
        int? offset = 0,
        UrlItemStatus? requestStatus = default,
        UrlItemType? requestType = default,
        CancellationToken cancellationToken = default);

    Task<List<UrlItem>> TakeRequestsAsync(
        int count,
        TimeSpan subtractTime,
        Guid serviceAccountId,
        int? offset = 0,
        UrlItemStatus? requestStatus = default,
        UrlItemType? requestType = default,
        CancellationToken cancellationToken = default);
}
