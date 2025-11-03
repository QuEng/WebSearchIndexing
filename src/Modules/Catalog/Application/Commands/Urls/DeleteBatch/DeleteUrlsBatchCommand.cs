namespace WebSearchIndexing.Modules.Catalog.Application.Commands.Urls.DeleteBatch;

public sealed record DeleteUrlsBatchCommand(IReadOnlyCollection<Guid> Ids);
