using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using WebSearchIndexing.Modules.Identity.Application.Abstractions;

namespace WebSearchIndexing.Modules.Identity.Api.Endpoints;

internal static class InvitationManagementEndpoints
{
    public static RouteGroupBuilder MapInvitationManagementEndpoints(this RouteGroupBuilder identityGroup)
    {
        ArgumentNullException.ThrowIfNull(identityGroup);

        var managementGroup = identityGroup.MapGroup("/invitation-management").RequireAuthorization();

        managementGroup.MapPost("/send", HandleSendInvitation);
        managementGroup.MapPost("/bulk-invite", HandleBulkInvite);
        managementGroup.MapPost("/{id:guid}/resend", HandleResendInvitation);
        managementGroup.MapGet("/tenant/{tenantId:guid}", HandleGetByTenant);

        return identityGroup;
    }

    private static async Task<IResult> HandleSendInvitation(
        SendInvitationRequest request,
        HttpContext context,
        IInvitationManagerService invitationManager,
        CancellationToken cancellationToken)
    {
        try
        {
            var userId = context.User.FindFirst("user_id")?.Value;
            if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userGuid))
            {
                return Results.Unauthorized();
            }

            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Role))
            {
                return Results.BadRequest(new { message = "Email and role are required" });
            }

            var result = await invitationManager.SendInvitationAsync(
                request.Email,
                userGuid,
                request.Role,
                request.TenantId,
                request.DomainId,
                cancellationToken);

            if (result.Success)
            {
                return Results.Ok(new { message = result.Message });
            }

            return Results.BadRequest(new { message = result.Message });
        }
        catch (Exception ex)
        {
            return Results.Problem(
                title: "Internal Server Error",
                detail: "An error occurred while sending the invitation",
                statusCode: 500);
        }
    }

    private static async Task<IResult> HandleBulkInvite(
        BulkInviteRequest request,
        HttpContext context,
        IInvitationManagerService invitationManager,
        CancellationToken cancellationToken)
    {
        try
        {
            var userId = context.User.FindFirst("user_id")?.Value;
            if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userGuid))
            {
                return Results.Unauthorized();
            }

            if (!request.Emails?.Any() == true || string.IsNullOrWhiteSpace(request.Role))
            {
                return Results.BadRequest(new { message = "Emails and role are required" });
            }

            var result = await invitationManager.BulkInviteAsync(
                request.Emails!,
                userGuid,
                request.Role,
                request.TenantId,
                request.DomainId,
                cancellationToken);

            if (result.Success)
            {
                return Results.Ok(new { message = result.Message });
            }

            return Results.BadRequest(new { message = result.Message });
        }
        catch (Exception ex)
        {
            return Results.Problem(
                title: "Internal Server Error",
                detail: "An error occurred while sending bulk invitations",
                statusCode: 500);
        }
    }

    private static async Task<IResult> HandleResendInvitation(
        Guid id,
        HttpContext context,
        IInvitationManagerService invitationManager,
        CancellationToken cancellationToken)
    {
        try
        {
            var userId = context.User.FindFirst("user_id")?.Value;
            if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userGuid))
            {
                return Results.Unauthorized();
            }

            var result = await invitationManager.ResendInvitationAsync(id, userGuid, cancellationToken);

            if (result.Success)
            {
                return Results.Ok(new { message = result.Message });
            }

            if (result.Message?.Contains("not found") == true)
            {
                return Results.NotFound(new { message = result.Message });
            }

            return Results.BadRequest(new { message = result.Message });
        }
        catch (Exception ex)
        {
            return Results.Problem(
                title: "Internal Server Error",
                detail: "An error occurred while resending the invitation",
                statusCode: 500);
        }
    }

    private static async Task<IResult> HandleGetByTenant(
        Guid tenantId,
        IInvitationManagerService invitationManager,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await invitationManager.GetInvitationsByTenantAsync(tenantId, cancellationToken);

            if (result.Success)
            {
                return Results.Ok(result.Invitations);
            }

            return Results.BadRequest(new { message = result.Message });
        }
        catch (Exception ex)
        {
            return Results.Problem(
                title: "Internal Server Error",
                detail: "An error occurred while retrieving tenant invitations",
                statusCode: 500);
        }
    }
}

public sealed record SendInvitationRequest(
    string Email,
    string Role,
    Guid? TenantId = null,
    Guid? DomainId = null);

public sealed record BulkInviteRequest(
    IEnumerable<string>? Emails,
    string Role,
    Guid? TenantId = null,
    Guid? DomainId = null);
