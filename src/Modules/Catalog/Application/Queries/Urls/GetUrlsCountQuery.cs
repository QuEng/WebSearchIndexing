using WebSearchIndexing.Modules.Catalog.Domain;

namespace WebSearchIndexing.Modules.Catalog.Application.Queries.Urls;

public sealed record GetUrlsCountQuery(
    UrlItemStatus? Status = null,
    UrlItemType? Type = null,
    Guid? ServiceAccountId = null,
    TimeSpan? SubtractTime = null);
