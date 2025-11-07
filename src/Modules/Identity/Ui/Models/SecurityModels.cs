namespace WebSearchIndexing.Modules.Identity.Ui.Models;

public sealed class LoginHistoryDto
{
    public Guid Id { get; set; }
    public string IpAddress { get; set; } = string.Empty;
    public string UserAgent { get; set; } = string.Empty;
    public string? Location { get; set; }
    public DateTime LoginAt { get; set; }
    public bool IsSuccessful { get; set; }
    public string? FailureReason { get; set; }
    public string? DeviceInfo { get; set; }
}

public sealed class ActiveSessionDto
{
    public Guid Id { get; set; }
    public string IpAddress { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public string DeviceType { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public bool IsCurrent { get; set; }
}
