using Microsoft.Extensions.Logging;
using WebSearchIndexing.Modules.Catalog.Application.Abstractions;
using WebSearchIndexing.Modules.Catalog.Application.DTOs;
using WebSearchIndexing.Modules.Catalog.Domain;

namespace WebSearchIndexing.Modules.Catalog.Application.Commands.Urls.UpdateStatus;

public sealed class UpdateUrlStatusHandler
{
    private readonly ICatalogRepository _repository;
    private readonly ILogger<UpdateUrlStatusHandler> _logger;

    public UpdateUrlStatusHandler(ICatalogRepository repository, ILogger<UpdateUrlStatusHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<UrlItemDto> HandleAsync(UpdateUrlStatusCommand command, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var urlItem = await _repository.GetUrlItemByIdAsync(command.UrlItemId, cancellationToken)
                      ?? throw new InvalidOperationException($"Url item with id '{command.UrlItemId}' was not found.");

        switch (command.Status)
        {
            case UrlItemStatus.Pending:
                urlItem.MarkPending();
                break;
            case UrlItemStatus.Completed:
            case UrlItemStatus.Failed:
                if (command.ServiceAccountId is null)
                {
                    throw new ArgumentException("Service account id is required to update url item status.", nameof(command));
                }

                var serviceAccount = await _repository.GetServiceAccountByIdAsync(command.ServiceAccountId.Value, cancellationToken)
                                     ?? throw new InvalidOperationException($"Service account with id '{command.ServiceAccountId}' was not found.");

                if (command.Status == UrlItemStatus.Completed)
                {
                    urlItem.MarkCompleted(serviceAccount);
                }
                else
                {
                    urlItem.MarkFailed(serviceAccount);
                }

                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(command.Status), command.Status, "Unsupported status value.");
        }

        await _repository.UpdateUrlItemAsync(urlItem, cancellationToken);

        _logger.LogInformation("Url item {UrlItemId} status updated to {Status}.", urlItem.Id, urlItem.Status);

        return UrlItemDto.FromDomain(urlItem);
    }
}
