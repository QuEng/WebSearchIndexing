namespace WebSearchIndexing.Modules.Identity.Ui.Models;

public sealed class TenantMemberDto
{
    public Guid UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public DateTime JoinedAt { get; set; }
    public bool IsActive { get; set; }

    public TenantMemberDto() { }

    public TenantMemberDto(
        Guid userId,
        string name,
        string email,
        string role,
        DateTime joinedAt,
        bool isActive)
    {
        UserId = userId;
        Name = name;
        Email = email;
        Role = role;
        JoinedAt = joinedAt;
        IsActive = isActive;
    }
}
