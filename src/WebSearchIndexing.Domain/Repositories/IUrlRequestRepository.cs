using WebSearchIndexing.Modules.Catalog.Domain;

namespace WebSearchIndexing.Domain.Repositories;

public interface IUrlRequestRepository : IRepository<UrlItem, Guid>
{
    Task<bool> AddRangeAsync(IEnumerable<UrlItem> urlRequests, CancellationToken cancellationToken = default);
    Task<bool> RemoveRangeAsync(IEnumerable<UrlItem> urlRequests);
    Task<List<UrlItem>> TakeRequestsAsync(int count, int? offset = 0, UrlItemStatus? requestStatus = default, UrlItemType? requestType = default, CancellationToken cancellationToken = default);
    Task<List<UrlItem>> TakeRequestsAsync(int count, Guid serviceAccountId, int? offset = 0, UrlItemStatus? requestStatus = default, UrlItemType? requestType = default, CancellationToken cancellationToken = default);
    Task<List<UrlItem>> TakeRequestsAsync(int count, TimeSpan subtructTime, int? offset = 0, UrlItemStatus? requestStatus = default, UrlItemType? requestType = default, CancellationToken cancellationToken = default);
    Task<List<UrlItem>> TakeRequestsAsync(int count, TimeSpan subtructTime, Guid serviceAccountId, int? offset = 0, UrlItemStatus? requestStatus = default, UrlItemType? requestType = default, CancellationToken cancellationToken = default);
    Task<int> GetRequestsCountAsync(UrlItemStatus? requestStatus = default, UrlItemType? requestType = default, CancellationToken cancellationToken = default);
    Task<int> GetRequestsCountAsync(Guid serviceAccountId, UrlItemStatus? requestStatus = default, UrlItemType? requestType = default, CancellationToken cancellationToken = default);
    Task<int> GetRequestsCountAsync(TimeSpan subtructTime, UrlItemStatus? requestStatus = default, UrlItemType? requestType = default, CancellationToken cancellationToken = default);
    Task<int> GetRequestsCountAsync(TimeSpan subtructTime, Guid serviceAccountId, UrlItemStatus? requestStatus = default, UrlItemType? requestType = default, CancellationToken cancellationToken = default);
}
