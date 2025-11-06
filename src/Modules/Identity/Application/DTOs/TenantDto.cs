namespace WebSearchIndexing.Modules.Identity.Application.DTOs;

public record TenantDto(
    Guid Id,
    string Name,
    string Slug,
    string Plan,
    DateTime CreatedAt,
    bool IsActive,
    Guid OwnerId,
    int MaxUsers,
    int MaxDomains,
    int DailyUrlQuota,
    int DailyInspectionQuota);
