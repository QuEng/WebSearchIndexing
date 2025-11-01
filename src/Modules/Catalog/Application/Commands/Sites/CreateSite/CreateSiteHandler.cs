using Microsoft.Extensions.Logging;
using WebSearchIndexing.Modules.Catalog.Application.Abstractions;
using WebSearchIndexing.Modules.Catalog.Application.DTOs;
using WebSearchIndexing.Modules.Catalog.Domain;

namespace WebSearchIndexing.Modules.Catalog.Application.Commands.Sites.CreateSite;

public sealed class CreateSiteHandler
{
    private readonly ICatalogRepository _repository;
    private readonly ILogger<CreateSiteHandler> _logger;

    public CreateSiteHandler(ICatalogRepository repository, ILogger<CreateSiteHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<SiteDto> HandleAsync(CreateSiteCommand command, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var normalizedHost = command.Host.Trim();
        if (string.IsNullOrWhiteSpace(normalizedHost))
        {
            throw new ArgumentException("Host is required.", nameof(command.Host));
        }

        var existing = await _repository.GetSiteByHostAsync(normalizedHost, cancellationToken);
        if (existing is not null)
        {
            _logger.LogInformation("Site with host {Host} already exists. Returning existing instance.", normalizedHost);
            return SiteDto.FromDomain(existing);
        }

        var site = new Site(normalizedHost);
        if (!string.IsNullOrWhiteSpace(command.DisplayName))
        {
            site.Rename(command.DisplayName);
        }

        var persisted = await _repository.AddSiteAsync(site, cancellationToken);
        _logger.LogInformation("Site {SiteId} created for host {Host}.", persisted.Id, normalizedHost);

        return SiteDto.FromDomain(persisted);
    }
}
