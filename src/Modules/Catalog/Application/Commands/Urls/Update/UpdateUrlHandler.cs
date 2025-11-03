using Microsoft.Extensions.Logging;
using WebSearchIndexing.Modules.Catalog.Application.Abstractions;
using WebSearchIndexing.Modules.Catalog.Application.DTOs;

namespace WebSearchIndexing.Modules.Catalog.Application.Commands.Urls.Update;

public sealed class UpdateUrlHandler
{
    private readonly IUrlRequestRepository _repository;
    private readonly ILogger<UpdateUrlHandler> _logger;

    public UpdateUrlHandler(IUrlRequestRepository repository, ILogger<UpdateUrlHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<UrlItemDto> HandleAsync(UpdateUrlCommand command, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var urlItem = await _repository.GetByIdAsync(command.Id, cancellationToken);
        if (urlItem == null)
        {
            throw new InvalidOperationException($"UrlItem with ID '{command.Id}' not found.");
        }

        urlItem.UpdateUrl(command.Url);
        urlItem.UpdatePriority(command.Priority);

        var success = await _repository.UpdateAsync(urlItem, cancellationToken);
        if (!success)
        {
            throw new InvalidOperationException($"Failed to update UrlItem with ID '{command.Id}'.");
        }

        _logger.LogInformation("UrlItem with ID '{Id}' was successfully updated.", command.Id);

        return UrlItemDto.FromDomain(urlItem);
    }
}
