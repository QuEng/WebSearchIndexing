using WebSearchIndexing.Modules.Catalog.Domain.Entities;

namespace WebSearchIndexing.Modules.Catalog.Domain.Repositories;

/// <summary>
/// Repository interface for managing service accounts
/// </summary>
public interface IServiceAccountRepository
{
    /// <summary>
    /// Adds a new service account
    /// </summary>
    /// <param name="entity">The service account to add</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The added service account</returns>
    Task<ServiceAccount> AddAsync(ServiceAccount entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing service account
    /// </summary>
    /// <param name="entity">The service account to update</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if update was successful, false otherwise</returns>
    Task<bool> UpdateAsync(ServiceAccount entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a service account by ID
    /// </summary>
    /// <param name="id">The service account ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if deletion was successful, false otherwise</returns>
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a service account by ID
    /// </summary>
    /// <param name="id">The service account ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The service account if found, null otherwise</returns>
    Task<ServiceAccount?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all service accounts
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of all service accounts</returns>
    Task<List<ServiceAccount>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a service account exists by ID
    /// </summary>
    /// <param name="id">The service account ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if exists, false otherwise</returns>
    Task<bool> EntityExistsAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a service account exists by project ID
    /// </summary>
    /// <param name="projectId">The Google project ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if exists, false otherwise</returns>
    Task<bool> EntityExistByProjectIdAsync(string projectId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the total count of service accounts
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Total count of service accounts</returns>
    Task<int> GetCountAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the total quota limit across all service accounts
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Total quota limit</returns>
    Task<int> GetQuotaByAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the available quota for today across all service accounts
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Available quota for today</returns>
    Task<int> GetQuotaAvailableTodayAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a service account that has available quota limit
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Service account with available limit, null if none available</returns>
    Task<ServiceAccount?> GetWithAvailableLimitAsync(CancellationToken cancellationToken = default);
}
