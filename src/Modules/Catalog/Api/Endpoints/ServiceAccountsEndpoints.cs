using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using WebSearchIndexing.Modules.Catalog.Application.Commands.ServiceAccounts;

namespace WebSearchIndexing.Modules.Catalog.Api;

internal static class ServiceAccountsEndpoints
{
    public static RouteGroupBuilder MapServiceAccountsEndpoints(this RouteGroupBuilder catalogGroup)
    {
        ArgumentNullException.ThrowIfNull(catalogGroup);

        catalogGroup.MapPost("/service-accounts", HandleAddServiceAccount);

        return catalogGroup;
    }

    private static async Task<IResult> HandleAddServiceAccount(
        AddServiceAccountCommand command,
        AddServiceAccountHandler handler,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await handler.HandleAsync(command, cancellationToken);
            return Results.Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return Results.Conflict(new { message = ex.Message });
        }
    }
}
