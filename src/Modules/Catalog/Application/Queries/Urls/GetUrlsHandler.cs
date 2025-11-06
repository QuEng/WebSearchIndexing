using WebSearchIndexing.Modules.Catalog.Domain.Repositories;
using WebSearchIndexing.Modules.Catalog.Application.DTOs;
using WebSearchIndexing.Modules.Catalog.Domain.Entities;

namespace WebSearchIndexing.Modules.Catalog.Application.Queries.Urls;

public sealed class GetUrlsHandler
{
    private readonly IUrlRequestRepository _repository;

    public GetUrlsHandler(IUrlRequestRepository repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyCollection<UrlItemDto>> HandleAsync(GetUrlsQuery query, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        List<UrlItem> urlItems;

        if (query.ServiceAccountId.HasValue && query.SubtractTime.HasValue)
        {
            urlItems = await _repository.TakeRequestsAsync(
                query.Count,
                query.SubtractTime.Value,
                query.ServiceAccountId.Value,
                query.Offset,
                query.Status,
                query.Type,
                cancellationToken);
        }
        else if (query.ServiceAccountId.HasValue)
        {
            urlItems = await _repository.TakeRequestsAsync(
                query.Count,
                query.ServiceAccountId.Value,
                query.Offset,
                query.Status,
                query.Type,
                cancellationToken);
        }
        else if (query.SubtractTime.HasValue)
        {
            urlItems = await _repository.TakeRequestsAsync(
                query.Count,
                query.SubtractTime.Value,
                query.Offset,
                query.Status,
                query.Type,
                cancellationToken);
        }
        else
        {
            urlItems = await _repository.TakeRequestsAsync(
                query.Count,
                query.Offset,
                query.Status,
                query.Type,
                cancellationToken);
        }

        return urlItems.Select(UrlItemDto.FromDomain).ToArray();
    }
}
