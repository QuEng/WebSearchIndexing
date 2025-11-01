using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using WebSearchIndexing.Modules.Catalog.Application.Commands.Urls.UpdateStatus;

namespace WebSearchIndexing.Modules.Catalog.Api;

internal static class UrlsEndpoints
{
    public static RouteGroupBuilder MapUrlsEndpoints(this RouteGroupBuilder catalogGroup)
    {
        ArgumentNullException.ThrowIfNull(catalogGroup);

        catalogGroup.MapPatch("/urls", HandleUpdateUrlStatus);

        catalogGroup.MapPost("/urls/status", HandleUpdateUrlStatus)
            .WithMetadata(new ObsoleteAttribute("Use PATCH /api/catalog/urls instead."));

        return catalogGroup;
    }

    private static async Task<IResult> HandleUpdateUrlStatus(
        UpdateUrlStatusCommand command,
        UpdateUrlStatusHandler handler,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await handler.HandleAsync(command, cancellationToken);
            return Results.Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return Results.NotFound(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(new { message = ex.Message });
        }
    }
}
