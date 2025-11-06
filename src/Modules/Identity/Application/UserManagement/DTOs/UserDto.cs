using WebSearchIndexing.Modules.Identity.Domain.Entities;

namespace WebSearchIndexing.Modules.Identity.Application.UserManagement.DTOs;

public sealed record UserDto(
    Guid Id,
    string Email,
    string FirstName,
    string LastName,
    bool IsActive,
    bool EmailVerified,
    DateTime CreatedAt,
    DateTime? LastLoginAt,
    IReadOnlyCollection<string> Roles)
{
    public string FullName => $"{FirstName} {LastName}".Trim();

    public static UserDto FromDomain(User user)
    {
        ArgumentNullException.ThrowIfNull(user);

        return new UserDto(
            user.Id,
            user.Email,
            user.FirstName,
            user.LastName,
            user.IsActive,
            user.IsEmailVerified,
            user.CreatedAt,
            user.LastLoginAt,
            Array.Empty<string>() // For now, until we implement roles relationship
        );
    }
}
