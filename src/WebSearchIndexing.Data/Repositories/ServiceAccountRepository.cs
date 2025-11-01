using Microsoft.EntityFrameworkCore;
using WebSearchIndexing.Domain.Repositories;
using WebSearchIndexing.Modules.Catalog.Domain;

namespace WebSearchIndexing.Data.Repositories;

public class ServiceAccountRepository(IDbContextFactory<IndexingDbContext> factory, IUrlRequestRepository urlRequestRepository) :
    BaseRepository<ServiceAccount, Guid>(factory),
    IServiceAccountRepository
{
    private readonly IDbContextFactory<IndexingDbContext> _factory = factory;
    private readonly IUrlRequestRepository _urlRequestRepository = urlRequestRepository;

    public override async Task<List<ServiceAccount>> GetAllAsync()
    {
        using var context = _factory.CreateDbContext();
        var serviceAccounts = await context.Set<ServiceAccount>()
            .Where(item => item.DeletedAt == null)
            .ToListAsync();

        foreach (var serviceAccount in serviceAccounts)
        {
            var usedToday = await _urlRequestRepository
                .GetRequestsCountAsync(TimeSpan.FromDays(1), serviceAccount.Id, UrlItemStatus.Completed);
            serviceAccount.LoadQuotaUsage((uint)usedToday);
        }

        return serviceAccounts;
    }

    public override async Task<bool> DeleteAsync(Guid id)
    {
        using var context = _factory.CreateDbContext();
        var entity = await context.Set<ServiceAccount>().FindAsync(id);

        if (entity is null)
        {
            return false;
        }

        entity.MarkDeleted();
        await context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> EntityExistByProjectIdAsync(string projectId)
    {
        using var context = _factory.CreateDbContext();
        return await context.Set<ServiceAccount>()
            .AnyAsync(item => item.ProjectId.Equals(projectId) && item.DeletedAt == null);
    }

    public async Task<ServiceAccount?> GetWithAvailableLimitAsync()
    {
        var serviceAccounts = await GetAllAsync();
        foreach (var serviceAccount in serviceAccounts)
        {
            if (serviceAccount.CanHandle(1))
            {
                return serviceAccount;
            }
        }

        return null;
    }

    public async Task<int> GetCountAsync()
    {
        using var context = _factory.CreateDbContext();
        return await context.Set<ServiceAccount>().CountAsync(item => item.DeletedAt == null);
    }

    public async Task<int> GetQuotaByAllAsync()
    {
        using var context = _factory.CreateDbContext();
        return (int)await context.Set<ServiceAccount>()
            .Where(item => item.DeletedAt == null)
            .SumAsync(item => item.QuotaLimitPerDay);
    }

    public async Task<int> GetQuotaAvailableTodayAsync()
    {
        var serviceAccounts = await GetAllAsync();
        return serviceAccounts.Sum(account => (int)account.RemainingQuota);
    }
}
