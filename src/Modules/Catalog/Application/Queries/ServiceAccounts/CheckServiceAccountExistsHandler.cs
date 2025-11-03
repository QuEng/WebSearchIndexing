using WebSearchIndexing.Modules.Catalog.Application.Abstractions;

namespace WebSearchIndexing.Modules.Catalog.Application.Queries.ServiceAccounts;

public sealed class CheckServiceAccountExistsHandler
{
    private readonly IServiceAccountRepository _repository;

    public CheckServiceAccountExistsHandler(IServiceAccountRepository repository)
    {
        _repository = repository;
    }

    public async Task<bool> HandleAsync(CheckServiceAccountExistsQuery query, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        return await _repository.EntityExistByProjectIdAsync(query.ProjectId);
    }
}
