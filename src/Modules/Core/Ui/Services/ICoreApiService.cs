using WebSearchIndexing.Modules.Core.Application.DTOs;

namespace WebSearchIndexing.Modules.Core.Ui.Services;

public interface ICoreApiService
{
    Task<SettingsDto?> GetSettingsAsync(CancellationToken cancellationToken = default);
    Task<SettingsDto> UpdateSettingsAsync(UpdateSettingsRequest request, CancellationToken cancellationToken = default);
    Task TriggerProcessingAsync(CancellationToken cancellationToken = default);
}

public record UpdateSettingsRequest(int RequestsPerDay, bool? IsEnabled = null);
