using WebSearchIndexing.Modules.Identity.Ui.Models;

namespace WebSearchIndexing.Modules.Identity.Ui.State;

public class UserState
{
    public Guid? UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime? CreatedAt { get; set; }
    public List<string> Roles { get; set; } = new();
    
    public string FullName => $"{FirstName} {LastName}".Trim();
    public string DisplayName => !string.IsNullOrEmpty(FullName) ? FullName : Email;
    
    public void UpdateFromProfile(UserProfileModel profile)
    {
        UserId = profile.Id;
        Email = profile.Email;
        FirstName = profile.FirstName;
        LastName = profile.LastName;
        IsActive = profile.IsActive;
        CreatedAt = profile.CreatedAt;
        Roles = profile.Roles.ToList();
    }
    
    public void Clear()
    {
        UserId = null;
        Email = string.Empty;
        FirstName = string.Empty;
        LastName = string.Empty;
        IsActive = false;
        CreatedAt = null;
        Roles.Clear();
    }
}
