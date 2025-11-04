using WebSearchIndexing.Modules.Catalog.Domain.Entities;

namespace WebSearchIndexing.Modules.Catalog.Application.Commands.Urls.UpdateStatus;

public sealed record UpdateUrlStatusCommand(
    Guid UrlItemId,
    UrlItemStatus Status,
    Guid? ServiceAccountId);
