using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using WebSearchIndexing.Modules.Identity.Application.Abstractions;
using WebSearchIndexing.Modules.Identity.Application.Services;

namespace WebSearchIndexing.Modules.Identity.Api.Endpoints;

internal static class InvitationEndpoints
{
    public static RouteGroupBuilder MapInvitationEndpoints(this RouteGroupBuilder identityGroup)
    {
        ArgumentNullException.ThrowIfNull(identityGroup);

        var invitationGroup = identityGroup.MapGroup("/invitations").RequireAuthorization();

        invitationGroup.MapGet("/incoming", HandleGetIncoming);
        invitationGroup.MapGet("/outgoing", HandleGetOutgoing);
        invitationGroup.MapGet("/{token}", HandleGetByToken);
        invitationGroup.MapPost("/{id:guid}/accept", HandleAccept);
        invitationGroup.MapPost("/{id:guid}/decline", HandleDecline);
        invitationGroup.MapPost("/{id:guid}/revoke", HandleRevoke);

        return identityGroup;
    }

    private static async Task<IResult> HandleGetIncoming(
        HttpContext context,
        IInvitationService invitationService,
        CancellationToken cancellationToken)
    {
        try
        {
            var userId = context.User.FindFirst("user_id")?.Value;
            if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userGuid))
            {
                return Results.Unauthorized();
            }

            var result = await invitationService.GetIncomingInvitationsAsync(userGuid, cancellationToken);

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
                detail: "An error occurred while retrieving incoming invitations",
                statusCode: 500);
        }
    }

    private static async Task<IResult> HandleGetOutgoing(
        HttpContext context,
        IInvitationService invitationService,
        CancellationToken cancellationToken)
    {
        try
        {
            var userId = context.User.FindFirst("user_id")?.Value;
            if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userGuid))
            {
                return Results.Unauthorized();
            }

            var result = await invitationService.GetOutgoingInvitationsAsync(userGuid, cancellationToken);

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
                detail: "An error occurred while retrieving outgoing invitations",
                statusCode: 500);
        }
    }

    private static async Task<IResult> HandleGetByToken(
        string token,
        IInvitationService invitationService,
        CancellationToken cancellationToken)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                return Results.BadRequest(new { message = "Token is required" });
            }

            var result = await invitationService.GetInvitationByTokenAsync(token, cancellationToken);

            if (result.Success)
            {
                return Results.Ok(result.Invitation);
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
                detail: "An error occurred while retrieving invitation details",
                statusCode: 500);
        }
    }

    private static async Task<IResult> HandleAccept(
        Guid id,
        HttpContext context,
        IInvitationService invitationService,
        CancellationToken cancellationToken)
    {
        try
        {
            var userId = context.User.FindFirst("user_id")?.Value;
            if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userGuid))
            {
                return Results.Unauthorized();
            }

            var result = await invitationService.AcceptInvitationAsync(id, userGuid, cancellationToken);

            if (result.Success)
            {
                return Results.Ok(new { message = "Invitation accepted successfully" });
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
                detail: "An error occurred while accepting the invitation",
                statusCode: 500);
        }
    }

    private static async Task<IResult> HandleDecline(
        Guid id,
        HttpContext context,
        IInvitationService invitationService,
        CancellationToken cancellationToken)
    {
        try
        {
            var userId = context.User.FindFirst("user_id")?.Value;
            if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userGuid))
            {
                return Results.Unauthorized();
            }

            var result = await invitationService.DeclineInvitationAsync(id, userGuid, cancellationToken);

            if (result.Success)
            {
                return Results.Ok(new { message = "Invitation declined successfully" });
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
                detail: "An error occurred while declining the invitation",
                statusCode: 500);
        }
    }

    private static async Task<IResult> HandleRevoke(
        Guid id,
        HttpContext context,
        IInvitationService invitationService,
        CancellationToken cancellationToken)
    {
        try
        {
            var userId = context.User.FindFirst("user_id")?.Value;
            if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userGuid))
            {
                return Results.Unauthorized();
            }

            var result = await invitationService.RevokeInvitationAsync(id, userGuid, cancellationToken);

            if (result.Success)
            {
                return Results.Ok(new { message = "Invitation revoked successfully" });
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
                detail: "An error occurred while revoking the invitation",
                statusCode: 500);
        }
    }
}
