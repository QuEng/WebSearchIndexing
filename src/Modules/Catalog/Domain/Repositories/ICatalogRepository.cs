using WebSearchIndexing.Modules.Catalog.Domain.Entities;

namespace WebSearchIndexing.Modules.Catalog.Domain.Repositories;

/// <summary>
/// Repository interface for catalog operations
/// </summary>
public interface ICatalogRepository
{
    /// <summary>
    /// Gets a site by its ID
    /// </summary>
    /// <param name="siteId">The site ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The site if found, null otherwise</returns>
    Task<Site?> GetSiteByIdAsync(Guid siteId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets a site by its host name
    /// </summary>
    /// <param name="host">The host name</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The site if found, null otherwise</returns>
    Task<Site?> GetSiteByHostAsync(string host, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Adds a new site to the catalog
    /// </summary>
    /// <param name="site">The site to add</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The added site</returns>
    Task<Site> AddSiteAsync(Site site, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a service account by its ID
    /// </summary>
    /// <param name="serviceAccountId">The service account ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The service account if found, null otherwise</returns>
    Task<ServiceAccount?> GetServiceAccountByIdAsync(Guid serviceAccountId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Checks if a service account exists by project ID
    /// </summary>
    /// <param name="projectId">The Google project ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if exists, false otherwise</returns>
    Task<bool> ServiceAccountExistsAsync(string projectId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Adds a new service account
    /// </summary>
    /// <param name="serviceAccount">The service account to add</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The added service account</returns>
    Task<ServiceAccount> AddServiceAccountAsync(ServiceAccount serviceAccount, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Updates an existing service account
    /// </summary>
    /// <param name="serviceAccount">The service account to update</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated service account</returns>
    Task<ServiceAccount> UpdateServiceAccountAsync(ServiceAccount serviceAccount, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a URL item by its ID
    /// </summary>
    /// <param name="urlItemId">The URL item ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The URL item if found, null otherwise</returns>
    Task<UrlItem?> GetUrlItemByIdAsync(Guid urlItemId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Adds multiple URL items to the catalog
    /// </summary>
    /// <param name="urlItems">The URL items to add</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task AddUrlItemsAsync(IEnumerable<UrlItem> urlItems, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Updates an existing URL item
    /// </summary>
    /// <param name="urlItem">The URL item to update</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task UpdateUrlItemAsync(UrlItem urlItem, CancellationToken cancellationToken = default);
}
