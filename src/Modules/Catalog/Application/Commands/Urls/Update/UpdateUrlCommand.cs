using WebSearchIndexing.Modules.Catalog.Domain;

namespace WebSearchIndexing.Modules.Catalog.Application.Commands.Urls.Update;

public sealed record UpdateUrlCommand(
    Guid Id,
    string Url,
    UrlItemPriority Priority);
