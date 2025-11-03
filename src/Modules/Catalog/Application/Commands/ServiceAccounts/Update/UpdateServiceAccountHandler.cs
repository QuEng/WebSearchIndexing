using Microsoft.Extensions.Logging;
using WebSearchIndexing.Modules.Catalog.Application.Abstractions;
using WebSearchIndexing.Modules.Catalog.Application.DTOs;

namespace WebSearchIndexing.Modules.Catalog.Application.Commands.ServiceAccounts.Update;

public sealed class UpdateServiceAccountHandler
{
    private readonly IServiceAccountRepository _repository;
    private readonly ILogger<UpdateServiceAccountHandler> _logger;

    public UpdateServiceAccountHandler(IServiceAccountRepository repository, ILogger<UpdateServiceAccountHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<ServiceAccountDto> HandleAsync(UpdateServiceAccountCommand command, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var serviceAccount = await _repository.GetByIdAsync(command.Id);
        if (serviceAccount == null)
        {
            throw new InvalidOperationException($"ServiceAccount with ID '{command.Id}' not found.");
        }

        serviceAccount.UpdateQuota(command.QuotaLimitPerDay);

        var success = await _repository.UpdateAsync(serviceAccount);
        if (!success)
        {
            throw new InvalidOperationException($"Failed to update ServiceAccount with ID '{command.Id}'.");
        }

        _logger.LogInformation("ServiceAccount with ID '{Id}' was successfully updated.", command.Id);

        return ServiceAccountDto.FromDomain(serviceAccount);
    }
}
