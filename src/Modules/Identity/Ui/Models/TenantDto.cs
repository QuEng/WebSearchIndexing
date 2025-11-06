namespace WebSearchIndexing.Modules.Identity.Ui.Models;

public sealed record TenantDto(
    Guid Id,
    string Name,
    string Plan,
    DateTime CreatedAt,
    bool IsActive,
    string UserRole)
{
    public static TenantDto Empty => new(
        Guid.Empty,
        string.Empty,
        string.Empty,
        DateTime.MinValue,
        false,
        string.Empty);
}
