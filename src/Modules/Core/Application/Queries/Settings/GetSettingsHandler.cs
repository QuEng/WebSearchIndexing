using WebSearchIndexing.Modules.Core.Application.DTOs;

namespace WebSearchIndexing.Modules.Core.Application.Queries.Settings;

public sealed class GetSettingsHandler
{
    private readonly ISettingsRepository _repository;

    public GetSettingsHandler(ISettingsRepository repository)
    {
        _repository = repository;
    }

    public async Task<SettingsDto?> HandleAsync(GetSettingsQuery query, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        var settings = await _repository.GetAsync();
        return settings != null ? SettingsDto.FromDomain(settings) : null;
    }
}
