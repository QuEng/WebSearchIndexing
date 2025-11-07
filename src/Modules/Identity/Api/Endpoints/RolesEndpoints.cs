using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using WebSearchIndexing.Modules.Identity.Domain.Entities;
using WebSearchIndexing.Modules.Identity.Domain.Repositories;
using WebSearchIndexing.Modules.Identity.Domain.Constants;

namespace WebSearchIndexing.Modules.Identity.Api.Endpoints;

internal static class RolesEndpoints
{
    public static RouteGroupBuilder MapRolesEndpoints(this RouteGroupBuilder identityGroup)
    {
        ArgumentNullException.ThrowIfNull(identityGroup);

        var rolesGroup = identityGroup.MapGroup("/roles").RequireAuthorization();

        // CRUD operations for roles
        rolesGroup.MapGet("/", HandleGetRoles);
        rolesGroup.MapPost("/", HandleCreateRole);
        rolesGroup.MapGet("/{roleId:guid}", HandleGetRole);
        rolesGroup.MapPut("/{roleId:guid}", HandleUpdateRole);
        rolesGroup.MapDelete("/{roleId:guid}", HandleDeleteRole);

        // User-role assignment endpoints
        rolesGroup.MapPost("/users/{userId:guid}/assign", HandleAssignUserRole);
        rolesGroup.MapDelete("/users/{userId:guid}/roles/{roleId:guid}", HandleRemoveUserRole);
        rolesGroup.MapGet("/users/{userId:guid}", HandleGetUserRoles);

        return identityGroup;
    }

    private static async Task<IResult> HandleGetRoles(
        HttpContext context,
        IRoleRepository roleRepository,
        CancellationToken cancellationToken)
    {
        try
        {
            // Check if user is GlobalAdmin
            if (!IsGlobalAdmin(context))
            {
                return Results.Forbid();
            }

            var typeParam = context.Request.Query["type"].ToString();
            IReadOnlyCollection<Role> roles;

            if (Enum.TryParse<RoleType>(typeParam, true, out var roleType))
            {
                roles = await roleRepository.GetByTypeAsync(roleType, cancellationToken);
            }
            else
            {
                roles = await roleRepository.GetAllAsync(cancellationToken);
            }

            var response = roles.Select(r => new RoleResponse(
                r.Id,
                r.Name,
                r.Type.ToString(),
                r.Description,
                r.Permissions,
                r.IsActive,
                r.CreatedAt
            )).ToList();

            return Results.Ok(response);
        }
        catch (Exception ex)
        {
            return Results.Problem($"Failed to get roles: {ex.Message}");
        }
    }

    private static async Task<IResult> HandleCreateRole(
        HttpContext context,
        IRoleRepository roleRepository,
        CancellationToken cancellationToken)
    {
        try
        {
            // Check if user is GlobalAdmin
            if (!IsGlobalAdmin(context))
            {
                return Results.Forbid();
            }

            var request = await context.Request.ReadFromJsonAsync<CreateRoleRequest>(cancellationToken);
            if (request == null)
            {
                return Results.BadRequest(new { message = "Invalid request body" });
            }

            // Validate role type
            if (!Enum.TryParse<RoleType>(request.Type, true, out var roleType))
            {
                return Results.BadRequest(new { message = "Invalid role type" });
            }

            // Check if role name already exists
            if (await roleRepository.ExistsByNameAsync(request.Name, cancellationToken))
            {
                return Results.BadRequest(new { message = "Role with this name already exists" });
            }

            // Validate permissions format
            var permissions = ValidateAndFormatPermissions(request.Permissions);

            var role = new Role(request.Name, roleType, request.Description, permissions);
            var savedRole = await roleRepository.AddAsync(role, cancellationToken);

            var response = new RoleResponse(
                savedRole.Id,
                savedRole.Name,
                savedRole.Type.ToString(),
                savedRole.Description,
                savedRole.Permissions,
                savedRole.IsActive,
                savedRole.CreatedAt
            );

            return Results.Ok(response);
        }
        catch (Exception ex)
        {
            return Results.Problem($"Failed to create role: {ex.Message}");
        }
    }

    private static async Task<IResult> HandleGetRole(
        HttpContext context,
        Guid roleId,
        IRoleRepository roleRepository,
        CancellationToken cancellationToken)
    {
        try
        {
            // Check if user is GlobalAdmin
            if (!IsGlobalAdmin(context))
            {
                return Results.Forbid();
            }

            var role = await roleRepository.GetByIdAsync(roleId, cancellationToken);
            if (role == null)
            {
                return Results.NotFound(new { message = "Role not found" });
            }

            var response = new RoleResponse(
                role.Id,
                role.Name,
                role.Type.ToString(),
                role.Description,
                role.Permissions,
                role.IsActive,
                role.CreatedAt
            );

            return Results.Ok(response);
        }
        catch (Exception ex)
        {
            return Results.Problem($"Failed to get role: {ex.Message}");
        }
    }

    private static async Task<IResult> HandleUpdateRole(
        HttpContext context,
        Guid roleId,
        IRoleRepository roleRepository,
        CancellationToken cancellationToken)
    {
        try
        {
            // Check if user is GlobalAdmin
            if (!IsGlobalAdmin(context))
            {
                return Results.Forbid();
            }

            var request = await context.Request.ReadFromJsonAsync<UpdateRoleRequest>(cancellationToken);
            if (request == null)
            {
                return Results.BadRequest(new { message = "Invalid request body" });
            }

            var role = await roleRepository.GetByIdAsync(roleId, cancellationToken);
            if (role == null)
            {
                return Results.NotFound(new { message = "Role not found" });
            }

            // Don't allow updating system roles (predefined roles)
            if (Roles.Validation.AllRoles.Contains(role.Name))
            {
                return Results.BadRequest(new { message = "Cannot modify system roles" });
            }

            // Validate permissions format
            var permissions = ValidateAndFormatPermissions(request.Permissions);

            role.UpdateDetails(request.Name, request.Description);
            role.UpdatePermissions(permissions);

            if (request.IsActive.HasValue)
            {
                if (request.IsActive.Value)
                    role.Activate();
                else
                    role.Deactivate();
            }

            await roleRepository.UpdateAsync(role, cancellationToken);

            return Results.Ok(new { message = "Role updated successfully" });
        }
        catch (Exception ex)
        {
            return Results.Problem($"Failed to update role: {ex.Message}");
        }
    }

    private static async Task<IResult> HandleDeleteRole(
        HttpContext context,
        Guid roleId,
        IRoleRepository roleRepository,
        CancellationToken cancellationToken)
    {
        try
        {
            // Check if user is GlobalAdmin
            if (!IsGlobalAdmin(context))
            {
                return Results.Forbid();
            }

            var role = await roleRepository.GetByIdAsync(roleId, cancellationToken);
            if (role == null)
            {
                return Results.NotFound(new { message = "Role not found" });
            }

            // Don't allow deleting system roles (predefined roles)
            if (Roles.Validation.AllRoles.Contains(role.Name))
            {
                return Results.BadRequest(new { message = "Cannot delete system roles" });
            }

            await roleRepository.DeleteAsync(roleId, cancellationToken);

            return Results.Ok(new { message = "Role deleted successfully" });
        }
        catch (Exception ex)
        {
            return Results.Problem($"Failed to delete role: {ex.Message}");
        }
    }

    private static async Task<IResult> HandleAssignUserRole(
        HttpContext context,
        Guid userId,
        IUserRepository userRepository,
        IRoleRepository roleRepository,
        CancellationToken cancellationToken)
    {
        try
        {
            // Check if user is GlobalAdmin
            if (!IsGlobalAdmin(context))
            {
                return Results.Forbid();
            }

            var request = await context.Request.ReadFromJsonAsync<AssignRoleRequest>(cancellationToken);
            if (request == null)
            {
                return Results.BadRequest(new { message = "Invalid request body" });
            }

            // Verify user exists
            var user = await userRepository.GetByIdAsync(userId, cancellationToken);
            if (user == null)
            {
                return Results.NotFound(new { message = "User not found" });
            }

            // Verify role exists and is global
            var role = await roleRepository.GetByIdAsync(request.RoleId, cancellationToken);
            if (role == null)
            {
                return Results.NotFound(new { message = "Role not found" });
            }

            if (role.Type != RoleType.Global)
            {
                return Results.BadRequest(new { message = "Only global roles can be assigned through this endpoint" });
            }

            // TODO: Implement user-role assignment logic
            // This would involve creating a UserRole entity or updating User entity with global roles

            return Results.Ok(new { message = "Role assigned successfully" });
        }
        catch (Exception ex)
        {
            return Results.Problem($"Failed to assign role: {ex.Message}");
        }
    }

    private static async Task<IResult> HandleRemoveUserRole(
        HttpContext context,
        Guid userId,
        Guid roleId,
        IUserRepository userRepository,
        CancellationToken cancellationToken)
    {
        try
        {
            // Check if user is GlobalAdmin
            if (!IsGlobalAdmin(context))
            {
                return Results.Forbid();
            }

            // TODO: Implement user-role removal logic

            return Results.Ok(new { message = "Role removed successfully" });
        }
        catch (Exception ex)
        {
            return Results.Problem($"Failed to remove user role: {ex.Message}");
        }
    }

    private static async Task<IResult> HandleGetUserRoles(
        HttpContext context,
        Guid userId,
        IUserRepository userRepository,
        CancellationToken cancellationToken)
    {
        try
        {
            // Check if user is GlobalAdmin or requesting own roles
            var currentUserId = context.User.FindFirst("user_id")?.Value;
            if (!IsGlobalAdmin(context) && (string.IsNullOrEmpty(currentUserId) || !Guid.TryParse(currentUserId, out var currentUserGuid) || currentUserGuid != userId))
            {
                return Results.Forbid();
            }

            // TODO: Implement get user roles logic
            var roles = new List<UserRoleResponse>();

            return Results.Ok(roles);
        }
        catch (Exception ex)
        {
            return Results.Problem($"Failed to get user roles: {ex.Message}");
        }
    }

    private static bool IsGlobalAdmin(HttpContext context)
    {
        // Check if user has GlobalAdmin role claim
        return context.User.HasClaim("role", Roles.GlobalAdmin);
    }

    private static string ValidateAndFormatPermissions(string? permissions)
    {
        if (string.IsNullOrWhiteSpace(permissions))
            return string.Empty;

        // Split, trim, validate, and rejoin permissions
        var permissionList = permissions.Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(p => p.Trim())
            .Where(p => Permissions.IsValid(p))
            .Distinct()
            .ToList();

        return string.Join(",", permissionList);
    }
}

// Request/Response models
public record CreateRoleRequest(
    string Name,
    string Type,
    string Description,
    string? Permissions = null);

public record UpdateRoleRequest(
    string Name,
    string Description,
    string? Permissions = null,
    bool? IsActive = null);

public record AssignRoleRequest(Guid RoleId);

public record RoleResponse(
    Guid Id,
    string Name,
    string Type,
    string Description,
    string Permissions,
    bool IsActive,
    DateTime CreatedAt);

public record UserRoleResponse(
    Guid UserId,
    Guid RoleId,
    string RoleName,
    string RoleType,
    DateTime AssignedAt);
