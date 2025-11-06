using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using WebSearchIndexing.Modules.Catalog.Domain.Repositories;
using WebSearchIndexing.Modules.Catalog.Application.Commands.ServiceAccounts;
using WebSearchIndexing.Modules.Catalog.Application.DTOs;

namespace WebSearchIndexing.Modules.Catalog.Api;

internal static class ServiceAccountsEndpoints
{
    /// <summary>
    /// Maps service account endpoints to the route group
    /// </summary>
    /// <param name="catalogGroup">Catalog route group builder</param>
    /// <returns>Configured route group builder</returns>
    public static RouteGroupBuilder MapServiceAccountsEndpoints(this RouteGroupBuilder catalogGroup)
    {
        ArgumentNullException.ThrowIfNull(catalogGroup);

        catalogGroup.MapGet("/service-accounts", HandleGetAllServiceAccounts);
        catalogGroup.MapGet("/service-accounts/{id:guid}", HandleGetServiceAccountById);
        catalogGroup.MapPost("/service-accounts", HandleAddServiceAccount);
        catalogGroup.MapPut("/service-accounts/{id:guid}", HandleUpdateServiceAccount);
        catalogGroup.MapDelete("/service-accounts/{id:guid}", HandleDeleteServiceAccount);
        catalogGroup.MapGet("/service-accounts/exists/{projectId}", HandleCheckServiceAccountExists);

        return catalogGroup;
    }

    private static async Task<IResult> HandleGetAllServiceAccounts(
        IServiceAccountRepository repository,
        CancellationToken cancellationToken)
    {
        try
        {
            var serviceAccounts = await repository.GetAllAsync();
            var dtos = serviceAccounts
                .OrderByDescending(x => x.CreatedAt)
                .Select(ServiceAccountDto.FromDomain)
                .Select(dto => dto.ToContract())
                .ToList();
            return Results.Ok(dtos);
        }
        catch (Exception ex)
        {
            return Results.Problem($"Failed to get service accounts: {ex.Message}");
        }
    }

    private static async Task<IResult> HandleGetServiceAccountById(
        Guid id,
        IServiceAccountRepository repository,
        CancellationToken cancellationToken)
    {
        try
        {
            var serviceAccount = await repository.GetByIdAsync(id);
            return serviceAccount is not null
                ? Results.Ok(ServiceAccountDto.FromDomain(serviceAccount).ToContract())
                : Results.NotFound(new { message = "Service account not found" });
        }
        catch (Exception ex)
        {
            return Results.Problem($"Failed to get service account: {ex.Message}");
        }
    }

    private static async Task<IResult> HandleUpdateServiceAccount(
        Guid id,
        Contracts.Catalog.UpdateServiceAccountRequest request,
        IServiceAccountRepository repository,
        CancellationToken cancellationToken)
    {
        try
        {
            var serviceAccount = await repository.GetByIdAsync(id);
            if (serviceAccount is null)
            {
                return Results.NotFound(new { message = "Service account not found" });
            }

            serviceAccount.UpdateQuota(request.QuotaLimitPerDay);
            var updated = await repository.UpdateAsync(serviceAccount);

            return updated
                ? Results.Ok(ServiceAccountDto.FromDomain(serviceAccount).ToContract())
                : Results.Problem("Failed to update service account");
        }
        catch (Exception ex)
        {
            return Results.Problem($"Failed to update service account: {ex.Message}");
        }
    }

    private static async Task<IResult> HandleDeleteServiceAccount(
        Guid id,
        IServiceAccountRepository repository,
        CancellationToken cancellationToken)
    {
        try
        {
            var deleted = await repository.DeleteAsync(id);
            return deleted
                ? Results.NoContent()
                : Results.NotFound(new { message = "Service account not found" });
        }
        catch (Exception ex)
        {
            return Results.Problem($"Failed to delete service account: {ex.Message}");
        }
    }

    private static async Task<IResult> HandleCheckServiceAccountExists(
        string projectId,
        IServiceAccountRepository repository,
        CancellationToken cancellationToken)
    {
        try
        {
            var exists = await repository.EntityExistByProjectIdAsync(projectId);
            return Results.Ok(new { exists });
        }
        catch (Exception ex)
        {
            return Results.Problem($"Failed to check service account existence: {ex.Message}");
        }
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
