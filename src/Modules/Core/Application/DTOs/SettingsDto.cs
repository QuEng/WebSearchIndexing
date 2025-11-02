using WebSearchIndexing.Modules.Core.Domain;

namespace WebSearchIndexing.Modules.Core.Application.DTOs;

public sealed record SettingsDto(
    Guid Id,
    Guid TenantId,
    string Key,
    int RequestsPerDay,
    bool IsEnabled)
{
    public static SettingsDto FromDomain(Settings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        return new SettingsDto(
            settings.Id,
            settings.TenantId,
            settings.Key,
            settings.RequestsPerDay,
            settings.IsEnabled);
    }
}
