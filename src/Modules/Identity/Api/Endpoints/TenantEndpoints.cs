using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using WebSearchIndexing.Modules.Identity.Application.Abstractions;
using WebSearchIndexing.Modules.Identity.Application.Services;

namespace WebSearchIndexing.Modules.Identity.Api.Endpoints;

internal static class TenantEndpoints
{
    public static RouteGroupBuilder MapTenantEndpoints(this RouteGroupBuilder identityGroup)
    {
        ArgumentNullException.ThrowIfNull(identityGroup);

        var tenantGroup = identityGroup.MapGroup("/tenants").RequireAuthorization();

        tenantGroup.MapGet("/", HandleGetTenants);
        tenantGroup.MapPost("/", HandleCreateTenant);
        tenantGroup.MapGet("/{tenantId:guid}", HandleGetTenant);
        tenantGroup.MapPut("/{tenantId:guid}", HandleUpdateTenant);
        tenantGroup.MapPost("/{tenantId:guid}/invite", HandleInviteUser);

        return identityGroup;
    }

    private static async Task<IResult> HandleGetTenants(
        HttpContext context,
        ITenantService tenantService,
        CancellationToken cancellationToken)
    {
        try
        {
            var userId = context.User.FindFirst("user_id")?.Value;
            if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userGuid))
            {
                return Results.Unauthorized();
            }

            var result = await tenantService.GetUserTenantsAsync(userGuid, cancellationToken);

            if (result.Success)
            {
                return Results.Ok(result.Tenants);
            }

            return Results.BadRequest(new { message = result.Message });
        }
        catch (Exception ex)
        {
            return Results.Problem($"Failed to get tenants: {ex.Message}");
        }
    }

    private static async Task<IResult> HandleCreateTenant(
        HttpContext context,
        ITenantService tenantService,
        CancellationToken cancellationToken)
    {
        try
        {
            var userId = context.User.FindFirst("user_id")?.Value;
            if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userGuid))
            {
                return Results.Unauthorized();
            }

            var request = await context.Request.ReadFromJsonAsync<CreateTenantRequest>(cancellationToken);
            if (request == null)
            {
                return Results.BadRequest(new { message = "Invalid request body" });
            }

            var createRequest = new Application.Services.CreateTenantRequest(request.Name, request.Plan);
            var result = await tenantService.CreateTenantAsync(userGuid, createRequest, cancellationToken);

            if (result.Success)
            {
                return Results.Ok(new { message = result.Message, tenantId = result.TenantId });
            }

            return Results.BadRequest(new { message = result.Message });
        }
        catch (Exception ex)
        {
            return Results.Problem($"Failed to create tenant: {ex.Message}");
        }
    }

    private static async Task<IResult> HandleGetTenant(
        HttpContext context,
        Guid tenantId,
        ITenantService tenantService,
        CancellationToken cancellationToken)
    {
        try
        {
            var userId = context.User.FindFirst("user_id")?.Value;
            if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userGuid))
            {
                return Results.Unauthorized();
            }

            var result = await tenantService.GetTenantAsync(tenantId, userGuid, cancellationToken);

            if (result.Success && result.Tenant != null)
            {
                return Results.Ok(new TenantResponse(
                    result.Tenant.Id,
                    result.Tenant.Name,
                    result.Tenant.Plan,
                    result.Tenant.CreatedAt,
                    result.Tenant.IsActive));
            }

            return Results.BadRequest(new { message = result.Message });
        }
        catch (Exception ex)
        {
            return Results.Problem($"Failed to get tenant: {ex.Message}");
        }
    }

    private static async Task<IResult> HandleUpdateTenant(
        HttpContext context,
        Guid tenantId,
        ITenantService tenantService,
        CancellationToken cancellationToken)
    {
        try
        {
            var userId = context.User.FindFirst("user_id")?.Value;
            if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userGuid))
            {
                return Results.Unauthorized();
            }

            var request = await context.Request.ReadFromJsonAsync<UpdateTenantRequest>(cancellationToken);
            if (request == null)
            {
                return Results.BadRequest(new { message = "Invalid request body" });
            }

            var updateRequest = new Application.Services.UpdateTenantRequest(request.Name, request.Plan);
            var result = await tenantService.UpdateTenantAsync(tenantId, userGuid, updateRequest, cancellationToken);

            if (result.Success)
            {
                return Results.Ok(new { message = result.Message });
            }

            return Results.BadRequest(new { message = result.Message });
        }
        catch (Exception ex)
        {
            return Results.Problem($"Failed to update tenant: {ex.Message}");
        }
    }

    private static async Task<IResult> HandleInviteUser(
        HttpContext context,
        Guid tenantId,
        ITenantService tenantService,
        CancellationToken cancellationToken)
    {
        try
        {
            var userId = context.User.FindFirst("user_id")?.Value;
            if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userGuid))
            {
                return Results.Unauthorized();
            }

            var request = await context.Request.ReadFromJsonAsync<InviteUserRequest>(cancellationToken);
            if (request == null)
            {
                return Results.BadRequest(new { message = "Invalid request body" });
            }

            var inviteRequest = new Application.Services.InviteUserRequest(request.Email, request.Role);
            var result = await tenantService.InviteUserToTenantAsync(tenantId, userGuid, inviteRequest, cancellationToken);

            if (result.Success)
            {
                return Results.Ok(new { message = result.Message });
            }

            return Results.BadRequest(new { message = result.Message });
        }
        catch (Exception ex)
        {
            return Results.Problem($"Failed to invite user: {ex.Message}");
        }
    }
}

// Request/Response models
public record CreateTenantRequest(string Name, string? Plan = null);
public record UpdateTenantRequest(string Name, string? Plan = null);
public record InviteUserRequest(string Email, string? Role = null);
public record TenantResponse(Guid Id, string Name, string? Plan, DateTime CreatedAt, bool IsActive);
