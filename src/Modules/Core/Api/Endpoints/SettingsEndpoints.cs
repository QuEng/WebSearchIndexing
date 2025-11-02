using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using WebSearchIndexing.Modules.Core.Application;
using WebSearchIndexing.Modules.Core.Application.DTOs;

namespace WebSearchIndexing.Modules.Core.Api;

internal static class SettingsEndpoints
{
    public static RouteGroupBuilder MapSettingsEndpoints(this RouteGroupBuilder coreGroup)
    {
        ArgumentNullException.ThrowIfNull(coreGroup);

        coreGroup.MapGet("/settings", HandleGetSettings);
        coreGroup.MapPut("/settings", HandleUpdateSettings);

        return coreGroup;
    }

    private static async Task<IResult> HandleGetSettings(
        ISettingsRepository repository,
        CancellationToken cancellationToken)
    {
        try
        {
            var settings = await repository.GetAsync();
            return settings is not null 
                ? Results.Ok(SettingsDto.FromDomain(settings)) 
                : Results.NotFound(new { message = "Settings not found" });
        }
        catch (Exception ex)
        {
            return Results.Problem($"Failed to get settings: {ex.Message}");
        }
    }

    private static async Task<IResult> HandleUpdateSettings(
        UpdateSettingsRequest request,
        ISettingsRepository repository,
        CancellationToken cancellationToken)
    {
        try
        {
            var settings = await repository.GetAsync();
            if (settings is null)
            {
                return Results.NotFound(new { message = "Settings not found" });
            }

            settings.RequestsPerDay = request.RequestsPerDay;
            if (request.IsEnabled.HasValue)
            {
                settings.IsEnabled = request.IsEnabled.Value;
            }
            
            var updated = await repository.UpdateAsync(settings);
            
            return updated 
                ? Results.Ok(SettingsDto.FromDomain(settings)) 
                : Results.Problem("Failed to update settings");
        }
        catch (Exception ex)
        {
            return Results.Problem($"Failed to update settings: {ex.Message}");
        }
    }
}

public record UpdateSettingsRequest(int RequestsPerDay, bool? IsEnabled = null);
