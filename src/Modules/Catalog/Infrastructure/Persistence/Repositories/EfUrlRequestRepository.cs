using Microsoft.EntityFrameworkCore;
using WebSearchIndexing.Modules.Catalog.Domain.Repositories;
using WebSearchIndexing.Modules.Catalog.Domain.Entities;

namespace WebSearchIndexing.Modules.Catalog.Infrastructure.Persistence.Repositories;

internal sealed class EfUrlRequestRepository : IUrlRequestRepository
{
    private readonly IDbContextFactory<CatalogDbContext> _contextFactory;

    public EfUrlRequestRepository(IDbContextFactory<CatalogDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<UrlItem> AddAsync(UrlItem entity, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entity);

        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        SetTenantId(context, entity);

        await context.UrlItems.AddAsync(entity, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        return entity;
    }

    public async Task<bool> UpdateAsync(UrlItem entity, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entity);

        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        SetTenantId(context, entity);

        context.UrlItems.Update(entity);
        var changes = await context.SaveChangesAsync(cancellationToken);

        return changes > 0;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

        var entity = await context.UrlItems
            .FirstOrDefaultAsync(item => item.Id == id, cancellationToken);

        if (entity is null)
        {
            return false;
        }

        context.UrlItems.Remove(entity);
        var changes = await context.SaveChangesAsync(cancellationToken);

        return changes > 0;
    }

    public async Task<UrlItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        return await context.UrlItems
            .AsNoTracking()
            .Include(item => item.ServiceAccount)
            .FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
    }

    public async Task<List<UrlItem>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        return await context.UrlItems
            .AsNoTracking()
            .Include(item => item.ServiceAccount)
            .OrderByDescending(item => item.Priority)
            .ThenBy(item => item.AddedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> AddRangeAsync(IEnumerable<UrlItem> urlRequests, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(urlRequests);

        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
            foreach (var urlRequest in urlRequests)
            {
                SetTenantId(context, urlRequest);
            }

            await context.UrlItems.AddRangeAsync(urlRequests, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> RemoveRangeAsync(IEnumerable<UrlItem> urlRequests, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(urlRequests);

        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
            context.UrlItems.RemoveRange(urlRequests);
            await context.SaveChangesAsync(cancellationToken);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<int> GetRequestsCountAsync(
        UrlItemStatus? requestStatus = default,
        UrlItemType? requestType = default,
        CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        return await context.UrlItems
            .Where(request => (requestStatus == null || request.Status == requestStatus) &&
                              (requestType == null || request.Type == requestType))
            .CountAsync(cancellationToken);
    }

    public async Task<int> GetRequestsCountAsync(
        Guid serviceAccountId,
        UrlItemStatus? requestStatus = default,
        UrlItemType? requestType = default,
        CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        return await context.UrlItems
            .Where(request => (requestStatus == null || request.Status == requestStatus) &&
                              (requestType == null || request.Type == requestType) &&
                              request.ServiceAccountId == serviceAccountId)
            .CountAsync(cancellationToken);
    }

    public async Task<int> GetRequestsCountAsync(
        TimeSpan subtractTime,
        UrlItemStatus? requestStatus = default,
        UrlItemType? requestType = default,
        CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

        var cutoff = DateTime.UtcNow - subtractTime;

        return await context.UrlItems
            .Where(request =>
                (requestStatus == null || request.Status == requestStatus) &&
                (requestType == null || request.Type == requestType) &&
                request.ProcessedAt >= cutoff)
            .CountAsync(cancellationToken);
    }

    public async Task<int> GetRequestsCountAsync(
        TimeSpan subtractTime,
        Guid serviceAccountId,
        UrlItemStatus? requestStatus = default,
        UrlItemType? requestType = default,
        CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

        var cutoff = DateTime.UtcNow - subtractTime;

        return await context.UrlItems
            .Where(request =>
                (requestStatus == null || request.Status == requestStatus) &&
                (requestType == null || request.Type == requestType) &&
                request.ServiceAccountId == serviceAccountId &&
                request.ProcessedAt >= cutoff)
            .CountAsync(cancellationToken);
    }

    public async Task<List<UrlItem>> TakeRequestsAsync(
        int count,
        int? offset = 0,
        UrlItemStatus? requestStatus = default,
        UrlItemType? requestType = default,
        CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        return await BuildBaseQuery(context, requestStatus, requestType, null)
            .Skip(offset ?? 0)
            .Take(count)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<UrlItem>> TakeRequestsAsync(
        int count,
        Guid serviceAccountId,
        int? offset = 0,
        UrlItemStatus? requestStatus = default,
        UrlItemType? requestType = default,
        CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        return await BuildBaseQuery(context, requestStatus, requestType, serviceAccountId)
            .Skip(offset ?? 0)
            .Take(count)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<UrlItem>> TakeRequestsAsync(
        int count,
        TimeSpan subtractTime,
        int? offset = 0,
        UrlItemStatus? requestStatus = default,
        UrlItemType? requestType = default,
        CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

        var cutoff = DateTime.UtcNow - subtractTime;

        return await BuildBaseQuery(context, requestStatus, requestType, null)
            .Where(request => request.ProcessedAt >= cutoff)
            .Skip(offset ?? 0)
            .Take(count)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<UrlItem>> TakeRequestsAsync(
        int count,
        TimeSpan subtractTime,
        Guid serviceAccountId,
        int? offset = 0,
        UrlItemStatus? requestStatus = default,
        UrlItemType? requestType = default,
        CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

        var cutoff = DateTime.UtcNow - subtractTime;

        return await BuildBaseQuery(context, requestStatus, requestType, serviceAccountId)
            .Where(request => request.ProcessedAt >= cutoff)
            .Skip(offset ?? 0)
            .Take(count)
            .ToListAsync(cancellationToken);
    }

    private static IQueryable<UrlItem> BuildBaseQuery(
        CatalogDbContext context,
        UrlItemStatus? requestStatus,
        UrlItemType? requestType,
        Guid? serviceAccountId)
    {
        var query = context.UrlItems
            .Include(request => request.ServiceAccount)
            .AsQueryable();

        if (requestStatus is not null)
        {
            query = query.Where(request => request.Status == requestStatus);
        }

        if (requestType is not null)
        {
            query = query.Where(request => request.Type == requestType);
        }

        if (serviceAccountId is not null)
        {
            query = query.Where(request => request.ServiceAccountId == serviceAccountId);
        }

        return query
            .OrderByDescending(request => request.Priority)
            .ThenBy(request => request.AddedAt);
    }

    private static void SetTenantId(DbContext context, UrlItem entity)
    {
        var tenantId = (context as CatalogDbContext)?.CurrentTenantId ?? Guid.Empty;
        context.Entry(entity).Property<Guid>("TenantId").CurrentValue = tenantId;
    }
}
