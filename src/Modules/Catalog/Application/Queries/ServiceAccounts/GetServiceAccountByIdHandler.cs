using WebSearchIndexing.Modules.Catalog.Application.Abstractions;
using WebSearchIndexing.Modules.Catalog.Application.DTOs;

namespace WebSearchIndexing.Modules.Catalog.Application.Queries.ServiceAccounts;

public sealed class GetServiceAccountByIdHandler
{
    private readonly IServiceAccountRepository _repository;

    public GetServiceAccountByIdHandler(IServiceAccountRepository repository)
    {
        _repository = repository;
    }

    public async Task<ServiceAccountDto?> HandleAsync(GetServiceAccountByIdQuery query, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        var serviceAccount = await _repository.GetByIdAsync(query.Id);
        return serviceAccount != null ? ServiceAccountDto.FromDomain(serviceAccount) : null;
    }
}
