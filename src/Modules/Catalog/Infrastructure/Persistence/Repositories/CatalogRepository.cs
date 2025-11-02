using Microsoft.EntityFrameworkCore;
using WebSearchIndexing.Modules.Catalog.Application.Abstractions;
using WebSearchIndexing.Modules.Catalog.Domain;

namespace WebSearchIndexing.Modules.Catalog.Infrastructure.Persistence.Repositories;

internal sealed class CatalogRepository : ICatalogRepository
{
    private readonly IDbContextFactory<CatalogDbContext> _contextFactory;
    private readonly IServiceAccountRepository _serviceAccountRepository;
    private readonly IUrlRequestRepository _urlRequestRepository;

    public CatalogRepository(
        IDbContextFactory<CatalogDbContext> contextFactory,
        IServiceAccountRepository serviceAccountRepository,
        IUrlRequestRepository urlRequestRepository)
    {
        _contextFactory = contextFactory;
        _serviceAccountRepository = serviceAccountRepository;
        _urlRequestRepository = urlRequestRepository;
    }

    public async Task<Site?> GetSiteByIdAsync(Guid siteId, CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        return await context.Sites
            .AsNoTracking()
            .FirstOrDefaultAsync(site => site.Id == siteId, cancellationToken);
    }

    public async Task<Site?> GetSiteByHostAsync(string host, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(host);

        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        return await context.Sites
            .AsNoTracking()
            .FirstOrDefaultAsync(site => site.Host == host, cancellationToken);
    }

    public async Task<Site> AddSiteAsync(Site site, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(site);

        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        SetTenantId(context, site);

        await context.Sites.AddAsync(site, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        return site;
    }

    public Task<ServiceAccount?> GetServiceAccountByIdAsync(Guid serviceAccountId, CancellationToken cancellationToken = default)
        => _serviceAccountRepository.GetByIdAsync(serviceAccountId, cancellationToken);

    public Task<bool> ServiceAccountExistsAsync(string projectId, CancellationToken cancellationToken = default)
        => _serviceAccountRepository.EntityExistByProjectIdAsync(projectId, cancellationToken);

    public Task<ServiceAccount> AddServiceAccountAsync(ServiceAccount serviceAccount, CancellationToken cancellationToken = default)
        => _serviceAccountRepository.AddAsync(serviceAccount, cancellationToken);

    public async Task<ServiceAccount> UpdateServiceAccountAsync(ServiceAccount serviceAccount, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(serviceAccount);

        await _serviceAccountRepository.UpdateAsync(serviceAccount, cancellationToken);
        return serviceAccount;
    }

    public Task<UrlItem?> GetUrlItemByIdAsync(Guid urlItemId, CancellationToken cancellationToken = default)
        => _urlRequestRepository.GetByIdAsync(urlItemId, cancellationToken);

    public async Task AddUrlItemsAsync(IEnumerable<UrlItem> urlItems, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(urlItems);
        await _urlRequestRepository.AddRangeAsync(urlItems, cancellationToken);
    }

    public async Task UpdateUrlItemAsync(UrlItem urlItem, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(urlItem);
        await _urlRequestRepository.UpdateAsync(urlItem, cancellationToken);
    }

    private static void SetTenantId(DbContext context, Site site)
    {
        var tenantId = (context as CatalogDbContext)?.CurrentTenantId ?? Guid.Empty;
        context.Entry(site).Property<Guid>("TenantId").CurrentValue = tenantId;
    }
}
