namespace WebSearchIndexing.Modules.Identity.Ui.Models;

public sealed record UserProfileDto(
    Guid Id,
    string Email,
    string FirstName,
    string LastName,
    DateTime CreatedAt,
    bool IsActive,
    bool IsEmailVerified);
