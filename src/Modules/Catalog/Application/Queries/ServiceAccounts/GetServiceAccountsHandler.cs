using WebSearchIndexing.Modules.Catalog.Domain.Repositories;
using WebSearchIndexing.Modules.Catalog.Application.DTOs;

namespace WebSearchIndexing.Modules.Catalog.Application.Queries.ServiceAccounts;

public sealed class GetServiceAccountsHandler
{
    private readonly IServiceAccountRepository _repository;

    public GetServiceAccountsHandler(IServiceAccountRepository repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyCollection<ServiceAccountDto>> HandleAsync(GetServiceAccountsQuery query, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        var serviceAccounts = await _repository.GetAllAsync();
        return serviceAccounts
            .OrderByDescending(x => x.CreatedAt)
            .Select(ServiceAccountDto.FromDomain)
            .ToArray();
    }
}
