using Microsoft.Extensions.Logging;
using WebSearchIndexing.Modules.Catalog.Application.Abstractions;
using WebSearchIndexing.Modules.Catalog.Application.DTOs;
using WebSearchIndexing.Modules.Catalog.Domain;

namespace WebSearchIndexing.Modules.Catalog.Application.Commands.ServiceAccounts;

public sealed class AddServiceAccountHandler
{
    private readonly ICatalogRepository _repository;
    private readonly ILogger<AddServiceAccountHandler> _logger;

    public AddServiceAccountHandler(ICatalogRepository repository, ILogger<AddServiceAccountHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<ServiceAccountDto> HandleAsync(AddServiceAccountCommand command, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (await _repository.ServiceAccountExistsAsync(command.ProjectId, cancellationToken))
        {
            throw new InvalidOperationException($"Service account with project id '{command.ProjectId}' already exists.");
        }

        var serviceAccount = new ServiceAccount(command.ProjectId, command.CredentialsJson, command.QuotaLimitPerDay);
        var persisted = await _repository.AddServiceAccountAsync(serviceAccount, cancellationToken);

        _logger.LogInformation("Service account {ServiceAccountId} created for project {ProjectId}.", persisted.Id, persisted.ProjectId);

        return ServiceAccountDto.FromDomain(persisted);
    }
}
