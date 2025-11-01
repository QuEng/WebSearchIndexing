using Microsoft.EntityFrameworkCore;
using WebSearchIndexing.Domain.Repositories;
using WebSearchIndexing.Modules.Catalog.Domain;

namespace WebSearchIndexing.Data.Repositories;

public class UrlRequestRepository(IDbContextFactory<IndexingDbContext> factory) :
    BaseRepository<UrlItem, Guid>(factory),
    IUrlRequestRepository
{
    private readonly IDbContextFactory<IndexingDbContext> _factory = factory;

    public async Task<bool> AddRangeAsync(IEnumerable<UrlItem> urlRequests, CancellationToken cancellationToken = default)
    {
        try
        {
            using var context = _factory.CreateDbContext();
            await context.Set<UrlItem>().AddRangeAsync(urlRequests, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> RemoveRangeAsync(IEnumerable<UrlItem> urlRequests)
    {
        try
        {
            using var context = _factory.CreateDbContext();
            context.Set<UrlItem>().RemoveRange(urlRequests);
            await context.SaveChangesAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<int> GetRequestsCountAsync(UrlItemStatus? requestStatus = default, UrlItemType? requestType = default, CancellationToken cancellationToken = default)
    {
        using var context = _factory.CreateDbContext();
        return await context.Set<UrlItem>()
            .Where(request => (requestStatus == null || request.Status == requestStatus) &&
                              (requestType == null || request.Type == requestType))
            .CountAsync(cancellationToken);
    }

    public async Task<int> GetRequestsCountAsync(Guid serviceAccountId, UrlItemStatus? requestStatus = default, UrlItemType? requestType = default, CancellationToken cancellationToken = default)
    {
        using var context = _factory.CreateDbContext();
        return await context.Set<UrlItem>()
            .Where(request => (requestStatus == null || request.Status == requestStatus) &&
                              (requestType == null || request.Type == requestType) &&
                              request.ServiceAccountId == serviceAccountId)
            .CountAsync(cancellationToken);
    }

    public async Task<int> GetRequestsCountAsync(TimeSpan subtructTime, UrlItemStatus? requestStatus = default, UrlItemType? requestType = default, CancellationToken cancellationToken = default)
    {
        using var context = _factory.CreateDbContext();
        return await context.Set<UrlItem>()
            .Where(request => (requestStatus == null || request.Status == requestStatus) &&
                              (requestType == null || request.Type == requestType) &&
                              request.ProcessedAt >= DateTime.UtcNow.Subtract(subtructTime))
            .CountAsync(cancellationToken);
    }

    public async Task<int> GetRequestsCountAsync(TimeSpan subtructTime, Guid serviceAccountId, UrlItemStatus? requestStatus = default, UrlItemType? requestType = default, CancellationToken cancellationToken = default)
    {
        using var context = _factory.CreateDbContext();
        return await context.Set<UrlItem>()
            .Where(request => (requestStatus == null || request.Status == requestStatus) &&
                              (requestType == null || request.Type == requestType) &&
                              request.ServiceAccountId == serviceAccountId &&
                              request.ProcessedAt >= DateTime.UtcNow.Subtract(subtructTime))
            .CountAsync(cancellationToken);
    }

    public async Task<List<UrlItem>> TakeRequestsAsync(int count, int? offset = 0, UrlItemStatus? requestStatus = null, UrlItemType? requestType = default, CancellationToken cancellationToken = default)
    {
        using var context = _factory.CreateDbContext();
        return await context.Set<UrlItem>()
            .Include(request => request.ServiceAccount)
            .Where(request => (requestStatus == null || request.Status == requestStatus) &&
                              (requestType == null || request.Type == requestType))
            .OrderByDescending(request => request.Priority)
            .ThenBy(request => request.AddedAt)
            .Skip(offset ?? 0)
            .Take(count)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<UrlItem>> TakeRequestsAsync(int count, Guid serviceAccountId, int? offset = 0, UrlItemStatus? requestStatus = null, UrlItemType? requestType = default, CancellationToken cancellationToken = default)
    {
        using var context = _factory.CreateDbContext();
        return await context.Set<UrlItem>()
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

    public async Task<List<UrlItem>> TakeRequestsAsync(int count, TimeSpan subtructTime, int? offset = 0, UrlItemStatus? requestStatus = null, UrlItemType? requestType = default, CancellationToken cancellationToken = default)
    {
        using var context = _factory.CreateDbContext();
        return await context.Set<UrlItem>()
            .Include(request => request.ServiceAccount)
            .Where(request => (requestStatus == null || request.Status == requestStatus) &&
                              (requestType == null || request.Type == requestType) &&
                              request.ProcessedAt >= DateTime.UtcNow.Subtract(subtructTime))
            .OrderByDescending(request => request.Priority)
            .ThenBy(request => request.AddedAt)
            .Skip(offset ?? 0)
            .Take(count)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<UrlItem>> TakeRequestsAsync(int count, TimeSpan subtructTime, Guid serviceAccountId, int? offset = 0, UrlItemStatus? requestStatus = null, UrlItemType? requestType = default, CancellationToken cancellationToken = default)
    {
        using var context = _factory.CreateDbContext();
        return await context.Set<UrlItem>()
            .Include(request => request.ServiceAccount)
            .Where(request => (requestStatus == null || request.Status == requestStatus) &&
                              (requestType == null || request.Type == requestType) &&
                              request.ServiceAccountId == serviceAccountId &&
                              request.ProcessedAt >= DateTime.UtcNow.Subtract(subtructTime))
            .OrderByDescending(request => request.Priority)
            .ThenBy(request => request.AddedAt)
            .Skip(offset ?? 0)
            .Take(count)
            .ToListAsync(cancellationToken);
    }
}
