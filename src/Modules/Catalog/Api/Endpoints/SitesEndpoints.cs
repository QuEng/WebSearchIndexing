using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using WebSearchIndexing.Modules.Catalog.Application.Commands.Sites.CreateSite;

namespace WebSearchIndexing.Modules.Catalog.Api;

internal static class SitesEndpoints
{
    public static RouteGroupBuilder MapSitesEndpoints(this RouteGroupBuilder catalogGroup)
    {
        ArgumentNullException.ThrowIfNull(catalogGroup);

        catalogGroup.MapPost("/sites", HandleCreateSite);

        return catalogGroup;
    }

    private static async Task<IResult> HandleCreateSite(
        HttpContext context,
        CreateSiteHandler handler,
        CancellationToken cancellationToken)
    {
        try
        {
            var command = await context.Request.ReadFromJsonAsync<CreateSiteCommand>(cancellationToken);
            if (command == null)
            {
                return Results.BadRequest(new { message = "Invalid request body." });
            }

            var result = await handler.HandleAsync(command, cancellationToken);
            return Results.Ok(result);
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(new { message = ex.Message });
        }
    }
}
