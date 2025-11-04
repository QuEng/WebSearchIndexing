using Microsoft.Extensions.Logging;
using WebSearchIndexing.Modules.Catalog.Application.Abstractions;
using WebSearchIndexing.Modules.Catalog.Application.DTOs;
using WebSearchIndexing.Modules.Catalog.Domain.Entities;

namespace WebSearchIndexing.Modules.Catalog.Application.Commands.Urls.Import;

public sealed class ImportUrlsHandler
{
    private readonly ICatalogRepository _repository;
    private readonly ILogger<ImportUrlsHandler> _logger;

    public ImportUrlsHandler(ICatalogRepository repository, ILogger<ImportUrlsHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<IReadOnlyCollection<UrlItemDto>> HandleAsync(ImportUrlsCommand command, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (command.Items is null || command.Items.Count == 0)
        {
            return Array.Empty<UrlItemDto>();
        }

        var urlItems = command.Items
            .Select(entry =>
            {
                if (string.IsNullOrWhiteSpace(entry.Url))
                {
                    throw new ArgumentException("Url is required for import.", nameof(command.Items));
                }

                return new UrlItem(entry.Url, entry.Type, entry.Priority);
            })
            .ToList();

        await _repository.AddUrlItemsAsync(urlItems, cancellationToken);

        _logger.LogInformation("{Count} url items imported{SiteInfo}.",
            urlItems.Count,
            command.SiteId.HasValue ? $" for site {command.SiteId}" : string.Empty);

        return urlItems.Select(UrlItemDto.FromDomain).ToArray();
    }
}
