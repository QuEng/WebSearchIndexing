using WebSearchIndexing.Modules.Core.Domain.Entities;

namespace WebSearchIndexing.Modules.Core.Domain.Repositories;

/// <summary>
/// Repository interface for managing application settings
/// </summary>
public interface ISettingsRepository
{
    /// <summary>
    /// Gets the application settings
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The settings entity</returns>
    Task<Settings> GetAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates the application settings
    /// </summary>
    /// <param name="settings">The settings to update</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if update was successful, false otherwise</returns>
    Task<bool> UpdateAsync(Settings settings, CancellationToken cancellationToken = default);
}
