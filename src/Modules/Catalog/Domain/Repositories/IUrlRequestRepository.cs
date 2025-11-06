using WebSearchIndexing.Modules.Catalog.Domain.Entities;

namespace WebSearchIndexing.Modules.Catalog.Domain.Repositories;

/// <summary>
/// Repository interface for managing URL items
/// </summary>
public interface IUrlRequestRepository
{
    /// <summary>
    /// Adds a new URL item
    /// </summary>
    /// <param name="entity">The URL item to add</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The added URL item</returns>
    Task<UrlItem> AddAsync(UrlItem entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing URL item
    /// </summary>
    /// <param name="entity">The URL item to update</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if update was successful, false otherwise</returns>
    Task<bool> UpdateAsync(UrlItem entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a URL item by ID
    /// </summary>
    /// <param name="id">The URL item ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if deletion was successful, false otherwise</returns>
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a URL item by ID
    /// </summary>
    /// <param name="id">The URL item ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The URL item if found, null otherwise</returns>
    Task<UrlItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all URL items
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of all URL items</returns>
    Task<List<UrlItem>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds multiple URL items
    /// </summary>
    /// <param name="urlRequests">The URL items to add</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if addition was successful, false otherwise</returns>
    Task<bool> AddRangeAsync(IEnumerable<UrlItem> urlRequests, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes multiple URL items
    /// </summary>
    /// <param name="urlRequests">The URL items to remove</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if removal was successful, false otherwise</returns>
    Task<bool> RemoveRangeAsync(IEnumerable<UrlItem> urlRequests, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the count of URL requests with optional filtering
    /// </summary>
    /// <param name="requestStatus">Optional status filter</param>
    /// <param name="requestType">Optional type filter</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Count of URL requests</returns>
    Task<int> GetRequestsCountAsync(
        UrlItemStatus? requestStatus = default,
        UrlItemType? requestType = default,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the count of URL requests for a specific service account
    /// </summary>
    /// <param name="serviceAccountId">The service account ID</param>
    /// <param name="requestStatus">Optional status filter</param>
    /// <param name="requestType">Optional type filter</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Count of URL requests</returns>
    Task<int> GetRequestsCountAsync(
        Guid serviceAccountId,
        UrlItemStatus? requestStatus = default,
        UrlItemType? requestType = default,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the count of URL requests within a specific time range
    /// </summary>
    /// <param name="subtractTime">Time to subtract from current time for filtering</param>
    /// <param name="requestStatus">Optional status filter</param>
    /// <param name="requestType">Optional type filter</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Count of URL requests</returns>
    Task<int> GetRequestsCountAsync(
        TimeSpan subtractTime,
        UrlItemStatus? requestStatus = default,
        UrlItemType? requestType = default,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the count of URL requests for a specific service account within a time range
    /// </summary>
    /// <param name="subtractTime">Time to subtract from current time for filtering</param>
    /// <param name="serviceAccountId">The service account ID</param>
    /// <param name="requestStatus">Optional status filter</param>
    /// <param name="requestType">Optional type filter</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Count of URL requests</returns>
    Task<int> GetRequestsCountAsync(
        TimeSpan subtractTime,
        Guid serviceAccountId,
        UrlItemStatus? requestStatus = default,
        UrlItemType? requestType = default,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a limited number of URL requests with optional filtering
    /// </summary>
    /// <param name="count">Maximum number of items to retrieve</param>
    /// <param name="offset">Number of items to skip</param>
    /// <param name="requestStatus">Optional status filter</param>
    /// <param name="requestType">Optional type filter</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of URL requests</returns>
    Task<List<UrlItem>> TakeRequestsAsync(
        int count,
        int? offset = 0,
        UrlItemStatus? requestStatus = default,
        UrlItemType? requestType = default,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a limited number of URL requests for a specific service account
    /// </summary>
    /// <param name="count">Maximum number of items to retrieve</param>
    /// <param name="serviceAccountId">The service account ID</param>
    /// <param name="offset">Number of items to skip</param>
    /// <param name="requestStatus">Optional status filter</param>
    /// <param name="requestType">Optional type filter</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of URL requests</returns>
    Task<List<UrlItem>> TakeRequestsAsync(
        int count,
        Guid serviceAccountId,
        int? offset = 0,
        UrlItemStatus? requestStatus = default,
        UrlItemType? requestType = default,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a limited number of URL requests within a specific time range
    /// </summary>
    /// <param name="count">Maximum number of items to retrieve</param>
    /// <param name="subtractTime">Time to subtract from current time for filtering</param>
    /// <param name="offset">Number of items to skip</param>
    /// <param name="requestStatus">Optional status filter</param>
    /// <param name="requestType">Optional type filter</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of URL requests</returns>
    Task<List<UrlItem>> TakeRequestsAsync(
        int count,
        TimeSpan subtractTime,
        int? offset = 0,
        UrlItemStatus? requestStatus = default,
        UrlItemType? requestType = default,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a limited number of URL requests for a specific service account within a time range
    /// </summary>
    /// <param name="count">Maximum number of items to retrieve</param>
    /// <param name="subtractTime">Time to subtract from current time for filtering</param>
    /// <param name="serviceAccountId">The service account ID</param>
    /// <param name="offset">Number of items to skip</param>
    /// <param name="requestStatus">Optional status filter</param>
    /// <param name="requestType">Optional type filter</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of URL requests</returns>
    Task<List<UrlItem>> TakeRequestsAsync(
        int count,
        TimeSpan subtractTime,
        Guid serviceAccountId,
        int? offset = 0,
        UrlItemStatus? requestStatus = default,
        UrlItemType? requestType = default,
        CancellationToken cancellationToken = default);
}
