using WebSearchIndexing.Domain.Entities;

namespace WebSearchIndexing.Domain.Repositories;

public interface IUrlRequestRepository : IRepository<UrlRequest, Guid>
{
    Task<bool> AddRangeAsync(IEnumerable<UrlRequest> urlRequests, CancellationToken cancellationToken = default);
    Task<bool> RemoveRangeAsync(IEnumerable<UrlRequest> urlRequests);
    Task<List<UrlRequest>> TakeRequestsAsync(int count, int? offset = 0, UrlRequestStatus? requestStatus = default, UrlRequestType? requestType = default, CancellationToken cancellationToken = default);
    Task<List<UrlRequest>> TakeRequestsAsync(int count, Guid serviceAccountId, int? offset = 0, UrlRequestStatus? requestStatus = default, UrlRequestType? requestType = default, CancellationToken cancellationToken = default);
    Task<List<UrlRequest>> TakeRequestsAsync(int count, TimeSpan subtructTime, int? offset = 0, UrlRequestStatus? requestStatus = default, UrlRequestType? requestType = default, CancellationToken cancellationToken = default);
    Task<List<UrlRequest>> TakeRequestsAsync(int count, TimeSpan subtructTime, Guid serviceAccountId, int? offset = 0, UrlRequestStatus? requestStatus = default, UrlRequestType? requestType = default, CancellationToken cancellationToken = default);
    Task<int> GetRequestsCountAsync(UrlRequestStatus? requestStatus = default, UrlRequestType? requestType = default, CancellationToken cancellationToken = default);
    Task<int> GetRequestsCountAsync(Guid serviceAccountId, UrlRequestStatus? requestStatus = default, UrlRequestType? requestType = default, CancellationToken cancellationToken = default);
    Task<int> GetRequestsCountAsync(TimeSpan subtructTime, UrlRequestStatus? requestStatus = default, UrlRequestType? requestType = default, CancellationToken cancellationToken = default);
    Task<int> GetRequestsCountAsync(TimeSpan subtructTime, Guid serviceAccountId, UrlRequestStatus? requestStatus = default, UrlRequestType? requestType = default, CancellationToken cancellationToken = default);
}