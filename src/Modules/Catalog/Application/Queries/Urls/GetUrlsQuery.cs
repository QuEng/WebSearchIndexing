using WebSearchIndexing.Modules.Catalog.Domain.Entities;

namespace WebSearchIndexing.Modules.Catalog.Application.Queries.Urls;

public sealed record GetUrlsQuery(
    int Count,
    int Offset = 0,
    UrlItemStatus? Status = null,
    UrlItemType? Type = null,
    Guid? ServiceAccountId = null,
    TimeSpan? SubtractTime = null);
