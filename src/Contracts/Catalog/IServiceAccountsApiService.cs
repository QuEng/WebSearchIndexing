namespace WebSearchIndexing.Contracts.Catalog;

/// <summary>
/// API service contract for managing service accounts
/// </summary>
public interface IServiceAccountsApiService
{
    /// <summary>
    /// Gets all service accounts
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of service account DTOs</returns>
    Task<IEnumerable<ServiceAccountDto>> GetAllAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets a service account by ID
    /// </summary>
    /// <param name="id">Service account ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Service account DTO if found, null otherwise</returns>
    Task<ServiceAccountDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Adds a new service account
    /// </summary>
    /// <param name="request">Add service account request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created service account DTO</returns>
    Task<ServiceAccountDto> AddAsync(AddServiceAccountRequest request, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Updates an existing service account
    /// </summary>
    /// <param name="id">Service account ID</param>
    /// <param name="request">Update service account request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated service account DTO</returns>
    Task<ServiceAccountDto> UpdateAsync(Guid id, UpdateServiceAccountRequest request, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Deletes a service account
    /// </summary>
    /// <param name="id">Service account ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Checks if a service account exists by project ID
    /// </summary>
    /// <param name="projectId">Google project ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if service account exists, false otherwise</returns>
    Task<bool> ExistsAsync(string projectId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Request for adding a new service account
/// </summary>
/// <param name="ProjectId">Google Cloud project ID</param>
/// <param name="KeyFilePath">Path to the service account key file</param>
/// <param name="QuotaLimitPerDay">Daily quota limit for the service account</param>
public record AddServiceAccountRequest(string ProjectId, string KeyFilePath, uint QuotaLimitPerDay);

/// <summary>
/// Request for updating an existing service account
/// </summary>
/// <param name="QuotaLimitPerDay">Updated daily quota limit for the service account</param>
public record UpdateServiceAccountRequest(uint QuotaLimitPerDay);
