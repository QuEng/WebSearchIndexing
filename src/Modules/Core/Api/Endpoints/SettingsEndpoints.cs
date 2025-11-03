using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using WebSearchIndexing.Modules.Core.Application.Queries.Settings;
using WebSearchIndexing.Modules.Core.Application.Commands.Settings;
using WebSearchIndexing.Modules.Core.Application.Commands.Processing;
using WebSearchIndexing.Modules.Core.Application.DTOs;

namespace WebSearchIndexing.Modules.Core.Api;

internal static class SettingsEndpoints
{
    public static RouteGroupBuilder MapSettingsEndpoints(this RouteGroupBuilder coreGroup)
    {
        ArgumentNullException.ThrowIfNull(coreGroup);

        coreGroup.MapGet("/settings", HandleGetSettings);
        coreGroup.MapPut("/settings", HandleUpdateSettings);
        coreGroup.MapPost("/settings/trigger-processing", HandleTriggerProcessing);

        return coreGroup;
    }

    private static async Task<IResult> HandleGetSettings(
        GetSettingsHandler handler,
        CancellationToken cancellationToken)
    {
        try
        {
            var query = new GetSettingsQuery();
            var settings = await handler.HandleAsync(query, cancellationToken);
            return settings is not null 
                ? Results.Ok(settings) 
                : Results.NotFound(new { message = "Settings not found" });
        }
        catch (Exception ex)
        {
            return Results.Problem($"Failed to get settings: {ex.Message}");
        }
    }

    private static async Task<IResult> HandleUpdateSettings(
        HttpContext context,
        UpdateSettingsHandler handler,
        CancellationToken cancellationToken)
    {
        try
        {
            var request = await context.Request.ReadFromJsonAsync<UpdateSettingsRequest>(cancellationToken);
            if (request == null)
            {
                return Results.BadRequest(new { message = "Invalid request body." });
            }

            var command = new UpdateSettingsCommand(request.RequestsPerDay, request.IsEnabled);
            var result = await handler.HandleAsync(command, cancellationToken);
            return Results.Ok(result);
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Results.NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return Results.Problem($"Failed to update settings: {ex.Message}");
        }
    }

    private static async Task<IResult> HandleTriggerProcessing(
        TriggerProcessingHandler handler,
        CancellationToken cancellationToken)
    {
        try
        {
            var command = new TriggerProcessingCommand();
            await handler.HandleAsync(command, cancellationToken);
            
            return Results.Ok(new { message = "Processing triggered successfully" });
        }
        catch (Exception ex)
        {
            return Results.Problem($"Failed to trigger processing: {ex.Message}");
        }
    }
}

public record UpdateSettingsRequest(int RequestsPerDay, bool? IsEnabled = null);
