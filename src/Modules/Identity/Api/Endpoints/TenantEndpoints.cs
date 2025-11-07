using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using WebSearchIndexing.Modules.Identity.Application.Abstractions;
using WebSearchIndexing.Modules.Identity.Domain.Repositories;
using WebSearchIndexing.Modules.Identity.Domain.Constants;

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
        
        // Tenant members management endpoints
        tenantGroup.MapGet("/{tenantId:guid}/members", HandleGetTenantMembers);
        tenantGroup.MapPut("/{tenantId:guid}/members/{userId:guid}/role", HandleUpdateMemberRole);
        tenantGroup.MapDelete("/{tenantId:guid}/members/{userId:guid}", HandleRemoveMember);

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
                    result.Tenant.IsActive,
                    result.Tenant.UserRole));
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

            var updateRequest = new Application.Services.UpdateTenantRequest(request.Name, request.Plan, request.IsActive);
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

    private static async Task<IResult> HandleGetTenantMembers(
        HttpContext context,
        Guid tenantId,
        IUserTenantRepository userTenantRepository,
        IUserRepository userRepository,
        CancellationToken cancellationToken)
    {
        try
        {
            var userId = context.User.FindFirst("user_id")?.Value;
            if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userGuid))
            {
                return Results.Unauthorized();
            }

            // Check if user has access to this tenant and is admin
            var userTenant = await userTenantRepository.GetByUserAndTenantAsync(userGuid, tenantId, cancellationToken);
            if (userTenant == null || !userTenant.IsActive)
            {
                return Results.Forbid();
            }

            // Get all members of the tenant
            var members = await userTenantRepository.GetByTenantAsync(tenantId, cancellationToken);
            var memberResponses = new List<TenantMemberResponse>();

            foreach (var member in members.Where(m => m.IsActive))
            {
                var user = await userRepository.GetByIdAsync(member.UserId, cancellationToken);
                if (user != null)
                {
                    memberResponses.Add(new TenantMemberResponse(
                        member.UserId,
                        $"{user.FirstName} {user.LastName}".Trim(),
                        user.Email,
                        member.Role,
                        member.CreatedAt,
                        member.IsActive));
                }
            }

            return Results.Ok(memberResponses);
        }
        catch (Exception ex)
        {
            return Results.Problem($"Failed to get tenant members: {ex.Message}");
        }
    }

    private static async Task<IResult> HandleUpdateMemberRole(
        HttpContext context,
        Guid tenantId,
        Guid userId,
        IUserTenantRepository userTenantRepository,
        CancellationToken cancellationToken)
    {
        try
        {
            var currentUserId = context.User.FindFirst("user_id")?.Value;
            if (string.IsNullOrEmpty(currentUserId) || !Guid.TryParse(currentUserId, out var currentUserGuid))
            {
                return Results.Unauthorized();
            }

            // Check if current user has admin rights
            var currentUserTenant = await userTenantRepository.GetByUserAndTenantAsync(currentUserGuid, tenantId, cancellationToken);
            if (currentUserTenant == null || !currentUserTenant.CanManageUsers())
            {
                return Results.Forbid();
            }

            var request = await context.Request.ReadFromJsonAsync<UpdateMemberRoleRequest>(cancellationToken);
            if (request == null || string.IsNullOrWhiteSpace(request.Role))
            {
                return Results.BadRequest(new { message = "Invalid role specified" });
            }

            // Validate role is a tenant role
            if (!Roles.Validation.IsOfType(request.Role, Domain.Entities.RoleType.Tenant))
            {
                return Results.BadRequest(new { message = "Invalid tenant role specified" });
            }

            // Get the member to update
            var memberTenant = await userTenantRepository.GetByUserAndTenantAsync(userId, tenantId, cancellationToken);
            if (memberTenant == null || !memberTenant.IsActive)
            {
                return Results.NotFound(new { message = "Member not found" });
            }

            // Don't allow changing your own role
            if (userId == currentUserGuid)
            {
                return Results.BadRequest(new { message = "Cannot change your own role" });
            }

            // Update the role
            memberTenant.UpdateRole(request.Role);
            await userTenantRepository.UpdateAsync(memberTenant, cancellationToken);

            return Results.Ok(new { message = "Member role updated successfully" });
        }
        catch (Exception ex)
        {
            return Results.Problem($"Failed to update member role: {ex.Message}");
        }
    }

    private static async Task<IResult> HandleRemoveMember(
        HttpContext context,
        Guid tenantId,
        Guid userId,
        IUserTenantRepository userTenantRepository,
        CancellationToken cancellationToken)
    {
        try
        {
            var currentUserId = context.User.FindFirst("user_id")?.Value;
            if (string.IsNullOrEmpty(currentUserId) || !Guid.TryParse(currentUserId, out var currentUserGuid))
            {
                return Results.Unauthorized();
            }

            // Check if current user has admin rights
            var currentUserTenant = await userTenantRepository.GetByUserAndTenantAsync(currentUserGuid, tenantId, cancellationToken);
            if (currentUserTenant == null || !currentUserTenant.CanManageUsers())
            {
                return Results.Forbid();
            }

            // Get the member to remove
            var memberTenant = await userTenantRepository.GetByUserAndTenantAsync(userId, tenantId, cancellationToken);
            if (memberTenant == null || !memberTenant.IsActive)
            {
                return Results.NotFound(new { message = "Member not found" });
            }

            // Don't allow removing yourself
            if (userId == currentUserGuid)
            {
                return Results.BadRequest(new { message = "Cannot remove yourself from the tenant" });
            }

            // Deactivate the member instead of deleting (soft delete)
            memberTenant.Deactivate();
            await userTenantRepository.UpdateAsync(memberTenant, cancellationToken);

            return Results.Ok(new { message = "Member removed successfully" });
        }
        catch (Exception ex)
        {
            return Results.Problem($"Failed to remove member: {ex.Message}");
        }
    }
}

// Request/Response models
public record CreateTenantRequest(string Name, string? Plan = null);
public record UpdateTenantRequest(string Name, string? Plan = null, bool IsActive = true);
public record InviteUserRequest(string Email, string? Role = null);
public record TenantResponse(Guid Id, string Name, string? Plan, DateTime CreatedAt, bool IsActive, string userRole);

// Tenant members management models
public record TenantMemberResponse(
    Guid UserId,
    string Name,
    string Email,
    string Role,
    DateTime JoinedAt,
    bool IsActive);

public record UpdateMemberRoleRequest(string Role);
