using WebSearchIndexing.Modules.Catalog.Domain.Entities;

namespace WebSearchIndexing.Modules.Catalog.Ui.Models;

public sealed record ImportUrlEntry(
    string Url,
    UrlItemType Type,
    UrlItemPriority Priority);
