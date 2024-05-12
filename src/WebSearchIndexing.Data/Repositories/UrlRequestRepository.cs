namespace WebSearchIndexing.Data.Repositories;

public class UrlRequestRepository(IDbContextFactory<IndexingDbContext> factory) :
    BaseRepository<UrlRequest, Guid>(factory),
    IUrlRequestRepository
{
    private readonly IDbContextFactory<IndexingDbContext> _factory = factory;

    public async Task<bool> AddRangeAsync(IEnumerable<UrlRequest> urlRequests, CancellationToken cancellationToken = default)
    {
        try
        {
            using var context = _factory.CreateDbContext();
            await context.Set<UrlRequest>().AddRangeAsync(urlRequests, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> RemoveRangeAsync(IEnumerable<UrlRequest> urlRequests)
    {
        try
        {
            using var context = _factory.CreateDbContext();
            context.Set<UrlRequest>().RemoveRange(urlRequests);
            await context.SaveChangesAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<int> GetRequestsCountAsync(UrlRequestStatus? requestStatus = default, UrlRequestType? requestType = default, CancellationToken cancellationToken = default)
    {
        using var context = _factory.CreateDbContext();
        return await context.Set<UrlRequest>()
            .Where(request => (requestStatus == null || request.Status == requestStatus) &&
                              (requestType == null || request.Type == requestType))
            .CountAsync(cancellationToken);
    }

    public async Task<int> GetRequestsCountAsync(Guid serviceAccountId, UrlRequestStatus? requestStatus = default, UrlRequestType? requestType = default, CancellationToken cancellationToken = default)
    {
        using var context = _factory.CreateDbContext();
        return await context.Set<UrlRequest>()
                .Where(request => (requestStatus == null || request.Status == requestStatus) &&
                                  (requestType == null || request.Type == requestType) &&
                                  request.ServiceAccountId == serviceAccountId)
                .CountAsync(cancellationToken);
    }

    public async Task<int> GetRequestsCountAsync(TimeSpan subtructTime, UrlRequestStatus? requestStatus = default, UrlRequestType? requestType = default, CancellationToken cancellationToken = default)
    {
        using var context = _factory.CreateDbContext();
        return await context.Set<UrlRequest>()
            .Where(request => (requestStatus == null || request.Status == requestStatus) &&
                              (requestType == null || request.Type == requestType) &&
                              request.ProcessedAt >= DateTime.UtcNow.AddMinutes(-subtructTime.TotalMinutes))
            .CountAsync(cancellationToken);
    }

    public async Task<int> GetRequestsCountAsync(TimeSpan subtructTime, Guid serviceAccountId, UrlRequestStatus? requestStatus = default, UrlRequestType? requestType = default, CancellationToken cancellationToken = default)
    {
        using var context = _factory.CreateDbContext();
        return await context.Set<UrlRequest>()
            .Where(request => (requestStatus == null || request.Status == requestStatus) &&
                              (requestType == null || request.Type == requestType) &&
                              request.ServiceAccountId == serviceAccountId &&
                              request.ProcessedAt >= DateTime.UtcNow.AddMinutes(-subtructTime.TotalMinutes))
            .CountAsync(cancellationToken);
    }

    public async Task<List<UrlRequest>> TakeRequestsAsync(int count, int? offset = 0, UrlRequestStatus? requestStatus = null, UrlRequestType? requestType = default, CancellationToken cancellationToken = default)
    {
        using var context = _factory.CreateDbContext();
        return await context.Set<UrlRequest>()
            .Include(request => request.ServiceAccount)
            .Where(request => (requestStatus == null || request.Status == requestStatus) &&
                              (requestType == null || request.Type == requestType))
            .OrderByDescending(request => request.Priority)
            .ThenBy(request => request.AddedAt)
            .Skip(offset ?? 0)
            .Take(count)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<UrlRequest>> TakeRequestsAsync(int count, Guid serviceAccountId, int? offset = 0, UrlRequestStatus? requestStatus = null, UrlRequestType? requestType = default, CancellationToken cancellationToken = default)
    {
        using var context = _factory.CreateDbContext();
        return await context.Set<UrlRequest>()
            .Include(request => request.ServiceAccount)
            .Where(request => (requestStatus == null || request.Status == requestStatus) &&
                              (requestType == null || request.Type == requestType) &&
                              request.ServiceAccountId == serviceAccountId)
            .OrderByDescending(request => request.Priority)
            .ThenBy(request => request.AddedAt)
            .Skip(offset ?? 0)
            .Take(count)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<UrlRequest>> TakeRequestsAsync(int count, TimeSpan subtructTime, int? offset = 0, UrlRequestStatus? requestStatus = null, UrlRequestType? requestType = default, CancellationToken cancellationToken = default)
    {
        using var context = _factory.CreateDbContext();
        return await context.Set<UrlRequest>()
            .Include(request => request.ServiceAccount)
            .Where(request => (requestStatus == null || request.Status == requestStatus) &&
                              (requestType == null || request.Type == requestType) &&
                              request.ProcessedAt >= DateTime.UtcNow.AddMinutes(-subtructTime.TotalMinutes))
            .OrderByDescending(request => request.Priority)
            .ThenBy(request => request.AddedAt)
            .Skip(offset ?? 0)
            .Take(count)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<UrlRequest>> TakeRequestsAsync(int count, TimeSpan subtructTime, Guid serviceAccountId, int? offset = 0, UrlRequestStatus? requestStatus = null, UrlRequestType? requestType = default, CancellationToken cancellationToken = default)
    {
        using var context = _factory.CreateDbContext();
        return await context.Set<UrlRequest>()
            .Include(request => request.ServiceAccount)
            .Where(request => (requestStatus == null || request.Status == requestStatus) &&
                              (requestType == null || request.Type == requestType) &&
                              request.ServiceAccountId == serviceAccountId &&
                              request.ProcessedAt >= DateTime.UtcNow.AddMinutes(-subtructTime.TotalMinutes))
            .OrderByDescending(request => request.Priority)
            .ThenBy(request => request.AddedAt)
            .Skip(offset ?? 0)
            .Take(count)
            .ToListAsync(cancellationToken);
    }
}