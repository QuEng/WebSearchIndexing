using Microsoft.Extensions.Logging;
using WebSearchIndexing.Modules.Catalog.Application.Abstractions;
using WebSearchIndexing.Modules.Catalog.Domain.Entities;

namespace WebSearchIndexing.Modules.Catalog.Application.Commands.Urls.DeleteBatch;

public sealed class DeleteUrlsBatchHandler
{
    private readonly IUrlRequestRepository _repository;
    private readonly ILogger<DeleteUrlsBatchHandler> _logger;

    public DeleteUrlsBatchHandler(IUrlRequestRepository repository, ILogger<DeleteUrlsBatchHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<bool> HandleAsync(DeleteUrlsBatchCommand command, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (command.Ids == null || command.Ids.Count == 0)
        {
            return true; // Nothing to delete
        }

        // Get all URL items by their IDs
        var urlItems = new List<UrlItem>();
        foreach (var id in command.Ids)
        {
            var urlItem = await _repository.GetByIdAsync(id, cancellationToken);
            if (urlItem != null)
            {
                urlItems.Add(urlItem);
            }
        }

        if (urlItems.Count == 0)
        {
            _logger.LogWarning("No UrlItems found for the provided IDs.");
            return false;
        }

        var success = await _repository.RemoveRangeAsync(urlItems, cancellationToken);
        
        if (success)
        {
            _logger.LogInformation("Successfully deleted {Count} UrlItems.", urlItems.Count);
        }
        else
        {
            _logger.LogWarning("Failed to delete batch of UrlItems.");
        }

        return success;
    }
}
