using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using WebSearchIndexing.Modules.Catalog.Application.Commands.Urls.Import;

namespace WebSearchIndexing.Modules.Catalog.Api;

internal static class BatchesEndpoints
{
    public static RouteGroupBuilder MapBatchesEndpoints(this RouteGroupBuilder catalogGroup)
    {
        ArgumentNullException.ThrowIfNull(catalogGroup);

        catalogGroup.MapPost("/batches", HandleImportUrls);

        catalogGroup.MapPost("/urls/import", HandleImportUrls)
            .WithMetadata(new ObsoleteAttribute("Use POST /api/catalog/batches instead."));

        return catalogGroup;
    }

    private static async Task<IResult> HandleImportUrls(
        ImportUrlsCommand command,
        ImportUrlsHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(command, cancellationToken);
        return Results.Ok(result);
    }
}
