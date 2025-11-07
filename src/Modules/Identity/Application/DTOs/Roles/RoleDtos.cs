using WebSearchIndexing.Modules.Identity.Domain.Entities;

namespace WebSearchIndexing.Modules.Identity.Application.DTOs.Roles;

public sealed record RoleDto(
    Guid Id,
    string Name,
    string Type,
    string Description,
    string Permissions,
    bool IsActive,
    DateTime CreatedAt)
{
    public static RoleDto FromDomain(Role role)
    {
        ArgumentNullException.ThrowIfNull(role);
        return new RoleDto(
            role.Id,
            role.Name,
            role.Type.ToString(),
            role.Description,
            role.Permissions,
            role.IsActive,
            role.CreatedAt
        );
    }
}

public sealed record RoleListItemDto(
    Guid Id,
    string Name,
    string Type,
    string Description,
    bool IsActive,
    int UserCount,
    DateTime CreatedAt)
{
    public static RoleListItemDto FromDomain(Role role, int userCount = 0)
    {
        ArgumentNullException.ThrowIfNull(role);
        return new RoleListItemDto(
            role.Id,
            role.Name,
            role.Type.ToString(),
            role.Description,
            role.IsActive,
            userCount,
            role.CreatedAt
        );
    }
}
