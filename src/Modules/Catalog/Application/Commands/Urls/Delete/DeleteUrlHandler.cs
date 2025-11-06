using Microsoft.Extensions.Logging;
using WebSearchIndexing.Modules.Catalog.Domain.Repositories;

namespace WebSearchIndexing.Modules.Catalog.Application.Commands.Urls.Delete;

public sealed class DeleteUrlHandler
{
    private readonly IUrlRequestRepository _repository;
    private readonly ILogger<DeleteUrlHandler> _logger;

    public DeleteUrlHandler(IUrlRequestRepository repository, ILogger<DeleteUrlHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<bool> HandleAsync(DeleteUrlCommand command, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var success = await _repository.DeleteAsync(command.Id, cancellationToken);
        
        if (success)
        {
            _logger.LogInformation("UrlItem with ID '{Id}' was successfully deleted.", command.Id);
        }
        else
        {
            _logger.LogWarning("Failed to delete UrlItem with ID '{Id}' or it was not found.", command.Id);
        }

        return success;
    }
}
