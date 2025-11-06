namespace WebSearchIndexing.Modules.Identity.Application.DTOs;

public record UserRoleDto(
    Guid UserId,
    Guid? TenantId,
    Guid? DomainId,
    string Role,
    string RoleType, // "Tenant" or "Domain"
    bool IsActive,
    DateTime CreatedAt);
