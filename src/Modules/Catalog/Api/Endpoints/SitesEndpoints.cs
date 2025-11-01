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
        CreateSiteCommand command,
        CreateSiteHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(command, cancellationToken);
        return Results.Ok(result);
    }
}
