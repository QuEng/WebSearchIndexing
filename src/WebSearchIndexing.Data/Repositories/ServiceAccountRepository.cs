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
        var serviceAccounts = await context.Set<ServiceAccount>().Where(item => item.DeletedAt == null).ToListAsync();
        foreach (var serviceAccount in serviceAccounts)
        {
            serviceAccount.QuotaLimitPerDayUsed = await _urlRequestRepository.GetRequestsCountAsync(TimeSpan.FromDays(1), serviceAccount.Id, UrlRequestStatus.Completed);
        }
        return serviceAccounts;
    }

    public override async Task<bool> DeleteAsync(Guid id)
    {
        using var context = _factory.CreateDbContext();
        var entity = context.Set<ServiceAccount>().Find(id);

        if (entity == null) return false;

        entity.DeletedAt = DateTime.UtcNow;
        await context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> EntityExistByProjectIdAsync(string projectId)
    {
        using var context = _factory.CreateDbContext();
        return await context.Set<ServiceAccount>().AnyAsync(item => item.ProjectId.Equals(projectId) && item.DeletedAt == null);
    }

    public async Task<ServiceAccount> GetWithAvailableLimitAsync()
    {
        var serviceAccounts = await GetAllAsync();
        foreach (var serviceAccount in serviceAccounts)
        {
            var requestCount = await _urlRequestRepository.GetRequestsCountAsync(TimeSpan.FromDays(1), serviceAccount.Id, UrlRequestStatus.Completed);
            if (requestCount < serviceAccount.QuotaLimitPerDay)
            {
                return serviceAccount;
            }
        }

        return null!;
    }

    public async Task<int> GetCountAsync()
    {
        using var context = _factory.CreateDbContext();
        return await context.Set<ServiceAccount>().CountAsync(item => item.DeletedAt == null);
    }

    public async Task<int> GetQuotaByAllAsync()
    {
        using var context = _factory.CreateDbContext();
        return (int)await context.Set<ServiceAccount>().Where(item => item.DeletedAt == null).SumAsync(item => item.QuotaLimitPerDay);
    }

    public async Task<int> GetQuotaAvailableTodayAsync()
    {
        int quotaAvailableToday = 0;
        var serviceAccounts = await GetAllAsync();
        foreach (var serviceAccount in serviceAccounts)
        {
            quotaAvailableToday += (int)serviceAccount.QuotaLimitPerDay - await _urlRequestRepository.GetRequestsCountAsync(TimeSpan.FromDays(1), serviceAccount.Id, UrlRequestStatus.Completed);
        }
        return quotaAvailableToday;
    }
}