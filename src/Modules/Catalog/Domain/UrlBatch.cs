using System;
using System.Collections.Generic;
using System.Linq;
using WebSearchIndexing.BuildingBlocks.Abstractions.Entities;

namespace WebSearchIndexing.Modules.Catalog.Domain;

public sealed class UrlBatch : IEntity<Guid>
{
    private readonly List<UrlItem> _items = [];

    private UrlBatch()
    {
        // For EF
    }

    public UrlBatch(Guid siteId, IEnumerable<UrlItem>? items = null)
    {
        Id = Guid.NewGuid();
        SiteId = siteId;
        CreatedAt = DateTime.UtcNow;

        if (items is not null)
        {
            _items.AddRange(items);
        }
    }

    public Guid Id { get; private set; }
    public Guid SiteId { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public IReadOnlyCollection<UrlItem> Items => _items.AsReadOnly();

    public UrlItem AddUrl(string url, UrlItemType type, UrlItemPriority priority)
    {
        var item = new UrlItem(url, type, priority);
        _items.Add(item);
        return item;
    }

    public bool RemoveUrl(Guid urlItemId) => _items.RemoveAll(item => item.Id == urlItemId) > 0;

    public UrlItem? Find(Guid urlItemId) => _items.FirstOrDefault(item => item.Id == urlItemId);

    public void MarkAllPending()
    {
        foreach (var url in _items)
        {
            url.MarkPending();
        }
    }
}
