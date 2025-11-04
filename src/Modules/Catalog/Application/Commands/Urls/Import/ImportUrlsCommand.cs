using WebSearchIndexing.Modules.Catalog.Domain.Entities;

namespace WebSearchIndexing.Modules.Catalog.Application.Commands.Urls.Import;

public sealed record ImportUrlsCommand(
    Guid? SiteId,
    IReadOnlyCollection<ImportUrlEntry> Items);

public sealed record ImportUrlEntry(
    string Url,
    UrlItemType Type,
    UrlItemPriority Priority);
