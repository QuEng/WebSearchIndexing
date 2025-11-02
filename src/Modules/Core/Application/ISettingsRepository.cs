using WebSearchIndexing.Modules.Core.Domain;

namespace WebSearchIndexing.Modules.Core.Application;

public interface ISettingsRepository
{
    Task<Settings> GetAsync(CancellationToken cancellationToken = default);

    Task<bool> UpdateAsync(Settings settings, CancellationToken cancellationToken = default);
}

