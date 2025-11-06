using WebSearchIndexing.Modules.Identity.Application.UserManagement.DTOs;
using WebSearchIndexing.Modules.Identity.Domain.Repositories;

namespace WebSearchIndexing.Modules.Identity.Application.UserManagement.Queries.GetUsers;

public sealed class GetUsersHandler
{
    private readonly IUserRepository _userRepository;

    public GetUsersHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
    }

    public async Task<GetUsersResult> HandleAsync(GetUsersQuery query, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        var users = await _userRepository.GetAllAsync(cancellationToken);

        // Apply filters
        var filteredUsers = users.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(query.SearchTerm))
        {
            var searchTerm = query.SearchTerm.ToLowerInvariant();
            filteredUsers = filteredUsers.Where(u => 
                u.Email.ToLowerInvariant().Contains(searchTerm) ||
                u.FirstName.ToLowerInvariant().Contains(searchTerm) ||
                u.LastName.ToLowerInvariant().Contains(searchTerm));
        }

        if (query.IsActive.HasValue)
        {
            filteredUsers = filteredUsers.Where(u => u.IsActive == query.IsActive.Value);
        }

        // Apply pagination
        var totalCount = filteredUsers.Count();
        var skip = (query.PageNumber - 1) * query.PageSize;
        var paginatedUsers = filteredUsers
            .Skip(skip)
            .Take(query.PageSize)
            .Select(UserDto.FromDomain)
            .ToList();

        return new GetUsersResult
        {
            Users = paginatedUsers,
            TotalCount = totalCount,
            PageNumber = query.PageNumber,
            PageSize = query.PageSize,
            TotalPages = (int)Math.Ceiling((double)totalCount / query.PageSize)
        };
    }
}
