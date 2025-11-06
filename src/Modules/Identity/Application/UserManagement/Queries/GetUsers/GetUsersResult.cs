using WebSearchIndexing.Modules.Identity.Application.UserManagement.DTOs;

namespace WebSearchIndexing.Modules.Identity.Application.UserManagement.Queries.GetUsers;

public sealed class GetUsersResult
{
    public IReadOnlyCollection<UserDto> Users { get; init; } = Array.Empty<UserDto>();
    public int TotalCount { get; init; }
    public int PageNumber { get; init; }
    public int PageSize { get; init; }
    public int TotalPages { get; init; }
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;
}
