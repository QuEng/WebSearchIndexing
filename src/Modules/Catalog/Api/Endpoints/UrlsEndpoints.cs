using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using WebSearchIndexing.Modules.Catalog.Application.Commands.Urls.Delete;
using WebSearchIndexing.Modules.Catalog.Application.Commands.Urls.DeleteBatch;
using WebSearchIndexing.Modules.Catalog.Application.Commands.Urls.Update;
using WebSearchIndexing.Modules.Catalog.Application.Commands.Urls.UpdateStatus;
using WebSearchIndexing.Modules.Catalog.Application.Queries.Urls;
using WebSearchIndexing.Modules.Catalog.Domain.Entities;

namespace WebSearchIndexing.Modules.Catalog.Api;

internal static class UrlsEndpoints
{
    public static RouteGroupBuilder MapUrlsEndpoints(this RouteGroupBuilder catalogGroup)
    {
        ArgumentNullException.ThrowIfNull(catalogGroup);

        catalogGroup.MapGet("/urls", HandleGetUrls);
        catalogGroup.MapGet("/urls/count", HandleGetUrlsCount);
        catalogGroup.MapPut("/urls/{id:guid}", HandleUpdateUrl);
        catalogGroup.MapDelete("/urls/{id:guid}", HandleDeleteUrl);
        catalogGroup.MapDelete("/urls/batch", HandleDeleteUrlsBatch);

        catalogGroup.MapPatch("/urls", HandleUpdateUrlStatus);

        catalogGroup.MapPost("/urls/status", HandleUpdateUrlStatus)
            .WithMetadata(new ObsoleteAttribute("Use PATCH /api/catalog/urls instead."));

        return catalogGroup;
    }

    private static async Task<IResult> HandleGetUrls(
        [FromQuery] int count = 10,
        [FromQuery] int offset = 0,
        [FromQuery] int? status = null,
        [FromQuery] int? type = null,
        [FromQuery] Guid? serviceAccountId = null,
        [FromQuery] int? subtractTimeHours = null,
        [FromServices] GetUrlsHandler handler = default!,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var query = new GetUrlsQuery(
                count,
                offset,
                status.HasValue ? (UrlItemStatus)status.Value : null,
                type.HasValue ? (UrlItemType)type.Value : null,
                serviceAccountId,
                subtractTimeHours.HasValue ? TimeSpan.FromHours(subtractTimeHours.Value) : null);

            var result = await handler.HandleAsync(query, cancellationToken);
            return Results.Ok(result);
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(new { message = ex.Message });
        }
    }

    private static async Task<IResult> HandleGetUrlsCount(
        [FromQuery] int? status = null,
        [FromQuery] int? type = null,
        [FromQuery] Guid? serviceAccountId = null,
        [FromQuery] int? subtractTimeHours = null,
        [FromServices] GetUrlsCountHandler handler = default!,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var query = new GetUrlsCountQuery(
                status.HasValue ? (UrlItemStatus)status.Value : null,
                type.HasValue ? (UrlItemType)type.Value : null,
                serviceAccountId,
                subtractTimeHours.HasValue ? TimeSpan.FromHours(subtractTimeHours.Value) : null);

            var result = await handler.HandleAsync(query, cancellationToken);
            return Results.Ok(new { count = result });
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(new { message = ex.Message });
        }
    }

    private static async Task<IResult> HandleUpdateUrl(
        Guid id,
        HttpContext context,
        [FromServices] UpdateUrlHandler handler,
        CancellationToken cancellationToken)
    {
        try
        {
            var command = await context.Request.ReadFromJsonAsync<UpdateUrlCommand>(cancellationToken);
            if (command == null)
            {
                return Results.BadRequest(new { message = "Invalid request body." });
            }

            if (id != command.Id)
            {
                return Results.BadRequest(new { message = "Route ID does not match command ID." });
            }

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

    private static async Task<IResult> HandleDeleteUrl(
        Guid id,
        [FromServices] DeleteUrlHandler handler,
        CancellationToken cancellationToken)
    {
        try
        {
            var command = new DeleteUrlCommand(id);
            var result = await handler.HandleAsync(command, cancellationToken);

            if (result)
            {
                return Results.NoContent();
            }

            return Results.NotFound(new { message = $"UrlItem with ID '{id}' not found." });
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(new { message = ex.Message });
        }
    }

    private static async Task<IResult> HandleDeleteUrlsBatch(
        HttpContext context,
        [FromServices] DeleteUrlsBatchHandler handler,
        CancellationToken cancellationToken)
    {
        try
        {
            var command = await context.Request.ReadFromJsonAsync<DeleteUrlsBatchCommand>(cancellationToken);
            if (command == null)
            {
                return Results.BadRequest(new { message = "Invalid request body." });
            }

            var result = await handler.HandleAsync(command, cancellationToken);

            if (result)
            {
                return Results.NoContent();
            }

            return Results.BadRequest(new { message = "Failed to delete some or all URLs." });
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(new { message = ex.Message });
        }
    }

    private static async Task<IResult> HandleUpdateUrlStatus(
        HttpContext context,
        [FromServices] UpdateUrlStatusHandler handler,
        CancellationToken cancellationToken)
    {
        try
        {
            var command = await context.Request.ReadFromJsonAsync<UpdateUrlStatusCommand>(cancellationToken);
            if (command == null)
            {
                return Results.BadRequest(new { message = "Invalid request body." });
            }

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
