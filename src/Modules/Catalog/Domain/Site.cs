using System;
using System.Collections.Generic;
using System.Linq;
using WebSearchIndexing.BuildingBlocks.Abstractions.Entities;

namespace WebSearchIndexing.Modules.Catalog.Domain;

public sealed class Site : IEntity<Guid>
{
    private readonly List<ServiceAccount> _serviceAccounts = [];
    private readonly List<UrlBatch> _batches = [];

    private Site()
    {
        // For EF
    }

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

    public void Rename(string displayName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(displayName);
        DisplayName = displayName.Trim();
    }

    public void AttachServiceAccount(ServiceAccount account)
    {
        ArgumentNullException.ThrowIfNull(account);
        if (_serviceAccounts.Any(existing => existing.Id == account.Id))
        {
            return;
        }

        _serviceAccounts.Add(account);
    }

    public void DetachServiceAccount(Guid serviceAccountId)
    {
        _serviceAccounts.RemoveAll(account => account.Id == serviceAccountId);
    }

    public UrlBatch StartBatch(IEnumerable<UrlItem>? items = null)
    {
        var batch = new UrlBatch(Id, items);
        _batches.Add(batch);
        return batch;
    }

    public UrlBatch? FindBatch(Guid batchId) => _batches.FirstOrDefault(batch => batch.Id == batchId);
}
