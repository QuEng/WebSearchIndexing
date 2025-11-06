namespace WebSearchIndexing.Modules.Identity.Ui.Models;

public class UserProfileModel
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public bool IsActive { get; set; }
    public bool IsEmailVerified { get; set; }
    public List<TenantInfo> Tenants { get; set; } = new();
    public List<string> Roles { get; set; } = new();
}

public class TenantInfo
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}
