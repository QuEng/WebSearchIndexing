namespace WebSearchIndexing.Modules.Identity.Application.UserManagement.Queries.GetUsers;

public sealed class GetUsersQuery
{
    public string? SearchTerm { get; init; }
    public string? Role { get; init; }
    public bool? IsActive { get; init; }
    public Guid? TenantId { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
}
