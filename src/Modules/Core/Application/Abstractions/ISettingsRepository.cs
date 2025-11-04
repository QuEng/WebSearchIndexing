using WebSearchIndexing.Modules.Core.Domain.Entities;

namespace WebSearchIndexing.Modules.Core.Application.Abstractions;

public interface ISettingsRepository
{
    Task<Settings> GetAsync(CancellationToken cancellationToken = default);

    Task<bool> UpdateAsync(Settings settings, CancellationToken cancellationToken = default);
}

