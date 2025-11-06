namespace WebSearchIndexing.Modules.Identity.Application.DTOs;

public record UserDto(
    Guid Id,
    string Email,
    string FirstName,
    string LastName,
    DateTime CreatedAt,
    bool IsActive,
    bool IsEmailVerified,
    DateTime? LastLoginAt);
