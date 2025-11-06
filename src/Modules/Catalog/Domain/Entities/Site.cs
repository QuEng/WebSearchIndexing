using System;
using System.Collections.Generic;
using System.Linq;
using WebSearchIndexing.BuildingBlocks.Abstractions.Entities;

namespace WebSearchIndexing.Modules.Catalog.Domain.Entities;

/// <summary>
/// Represents a website or domain in the catalog
/// </summary>
public sealed class Site : IEntity<Guid>
{
    private readonly List<ServiceAccount> _serviceAccounts = [];
    private readonly List<UrlBatch> _batches = [];

    private Site()
    {
        // For EF
    }

    /// <summary>
    /// Initializes a new instance of the Site class
    /// </summary>
    /// <param name="host">The host name or domain of the site</param>
    /// <exception cref="ArgumentException">Thrown when host is null or whitespace</exception>
    public Site(string host)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(host);

        Id = Guid.NewGuid();
        Host = host.Trim();
        CreatedAt = DateTime.UtcNow;
    }

    public Guid Id { get; private set; }
    public string Host { get; private set; } = string.Empty;
    public string? DisplayName { get; private set; }
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

    public IReadOnlyCollection<ServiceAccount> ServiceAccounts => _serviceAccounts.AsReadOnly();
    public IReadOnlyCollection<UrlBatch> UrlBatches => _batches.AsReadOnly();

    /// <summary>
    /// Renames the site with a new display name
    /// </summary>
    /// <param name="displayName">The new display name for the site</param>
    /// <exception cref="ArgumentException">Thrown when displayName is null or whitespace</exception>
    public void Rename(string displayName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(displayName);
        DisplayName = displayName.Trim();
    }

    /// <summary>
    /// Attaches a service account to this site
    /// </summary>
    /// <param name="account">The service account to attach</param>
    /// <exception cref="ArgumentNullException">Thrown when account is null</exception>
    public void AttachServiceAccount(ServiceAccount account)
    {
        ArgumentNullException.ThrowIfNull(account);
        if (_serviceAccounts.Any(existing => existing.Id == account.Id))
        {
            return;
        }

        _serviceAccounts.Add(account);
    }

    /// <summary>
    /// Detaches a service account from this site
    /// </summary>
    /// <param name="serviceAccountId">The ID of the service account to detach</param>
    public void DetachServiceAccount(Guid serviceAccountId)
    {
        _serviceAccounts.RemoveAll(account => account.Id == serviceAccountId);
    }

    /// <summary>
    /// Starts a new URL batch for this site
    /// </summary>
    /// <param name="items">Optional initial URL items for the batch</param>
    /// <returns>The created URL batch</returns>
    public UrlBatch StartBatch(IEnumerable<UrlItem>? items = null)
    {
        var batch = new UrlBatch(Id, items);
        _batches.Add(batch);
        return batch;
    }

    /// <summary>
    /// Finds a URL batch by its ID
    /// </summary>
    /// <param name="batchId">The ID of the batch to find</param>
    /// <returns>The URL batch if found, null otherwise</returns>
    public UrlBatch? FindBatch(Guid batchId) => _batches.FirstOrDefault(batch => batch.Id == batchId);
}
