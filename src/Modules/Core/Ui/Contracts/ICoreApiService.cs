using WebSearchIndexing.Modules.Core.Application.DTOs;
using WebSearchIndexing.Modules.Core.Ui.Models;

namespace WebSearchIndexing.Modules.Core.Ui.Contracts;

public interface ICoreApiService
{
    Task<SettingsDto?> GetSettingsAsync(CancellationToken cancellationToken = default);
    Task<SettingsDto> UpdateSettingsAsync(UpdateSettingsRequest request, CancellationToken cancellationToken = default);
    Task TriggerProcessingAsync(CancellationToken cancellationToken = default);
}
