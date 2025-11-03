using WebSearchIndexing.Modules.Catalog.Application.Abstractions;

namespace WebSearchIndexing.Modules.Catalog.Application.Queries.Urls;

public sealed class GetUrlsCountHandler
{
    private readonly IUrlRequestRepository _repository;

    public GetUrlsCountHandler(IUrlRequestRepository repository)
    {
        _repository = repository;
    }

    public async Task<int> HandleAsync(GetUrlsCountQuery query, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        if (query.ServiceAccountId.HasValue && query.SubtractTime.HasValue)
        {
            return await _repository.GetRequestsCountAsync(
                query.SubtractTime.Value,
                query.ServiceAccountId.Value,
                query.Status,
                query.Type,
                cancellationToken);
        }
        
        if (query.ServiceAccountId.HasValue)
        {
            return await _repository.GetRequestsCountAsync(
                query.ServiceAccountId.Value,
                query.Status,
                query.Type,
                cancellationToken);
        }
        
        if (query.SubtractTime.HasValue)
        {
            return await _repository.GetRequestsCountAsync(
                query.SubtractTime.Value,
                query.Status,
                query.Type,
                cancellationToken);
        }

        return await _repository.GetRequestsCountAsync(
            query.Status,
            query.Type,
            cancellationToken);
    }
}
