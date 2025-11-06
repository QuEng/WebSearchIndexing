namespace WebSearchIndexing.Modules.Identity.Ui.Models;

public sealed record InvitationDto(
    Guid Id,
    string Email,
    string TenantName,
    string Role,
    DateTime SentDate,
    DateTime ExpiresAt,
    string Status,
    string? InvitedBy)
{
    public static InvitationDto Empty => new(
        Guid.Empty,
        string.Empty,
        string.Empty,
        string.Empty,
        DateTime.MinValue,
        DateTime.MinValue,
        string.Empty,
        null);
        
    public bool IsExpired => DateTime.UtcNow > ExpiresAt;
    public bool IsPending => Status == "Pending";
    public bool IsAccepted => Status == "Accepted";
    public bool IsDeclined => Status == "Declined";
    public bool IsRevoked => Status == "Revoked";
}
