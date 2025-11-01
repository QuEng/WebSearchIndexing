using WebSearchIndexing.Modules.Catalog.Domain;

namespace WebSearchIndexing.Modules.Catalog.Application.Abstractions;

public interface ICatalogRepository
{
    Task<Site?> GetSiteByIdAsync(Guid siteId, CancellationToken cancellationToken = default);
    Task<Site?> GetSiteByHostAsync(string host, CancellationToken cancellationToken = default);
    Task<Site> AddSiteAsync(Site site, CancellationToken cancellationToken = default);

    Task<ServiceAccount?> GetServiceAccountByIdAsync(Guid serviceAccountId, CancellationToken cancellationToken = default);
    Task<bool> ServiceAccountExistsAsync(string projectId, CancellationToken cancellationToken = default);
    Task<ServiceAccount> AddServiceAccountAsync(ServiceAccount serviceAccount, CancellationToken cancellationToken = default);
    Task<ServiceAccount> UpdateServiceAccountAsync(ServiceAccount serviceAccount, CancellationToken cancellationToken = default);

    Task<UrlItem?> GetUrlItemByIdAsync(Guid urlItemId, CancellationToken cancellationToken = default);
    Task AddUrlItemsAsync(IEnumerable<UrlItem> urlItems, CancellationToken cancellationToken = default);
    Task UpdateUrlItemAsync(UrlItem urlItem, CancellationToken cancellationToken = default);
}
