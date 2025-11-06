using WebSearchIndexing.Modules.Identity.Domain.Entities;

namespace WebSearchIndexing.Modules.Identity.Application.DTOs;

public sealed class InvitationDto
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string TenantName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public DateTime SentDate { get; set; }
    public DateTime ExpiresAt { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? InvitedBy { get; set; }
    public Guid? TenantId { get; set; }
    public string? InvitationToken { get; set; }

    public bool IsExpired => DateTime.UtcNow > ExpiresAt;
    public bool IsPending => Status == "Pending";
    public bool IsAccepted => Status == "Accepted";
    public bool IsDeclined => Status == "Declined";
    public bool IsRevoked => Status == "Revoked";

    public static InvitationDto FromDomain(UserInvitation invitation, string? tenantName = null, string? invitedByName = null)
    {
        ArgumentNullException.ThrowIfNull(invitation);

        return new InvitationDto
        {
            Id = invitation.Id,
            Email = invitation.Email,
            TenantName = tenantName ?? "Unknown",
            Role = invitation.Role,
            SentDate = invitation.CreatedAt,
            ExpiresAt = invitation.ExpiresAt,
            Status = GetStatusText(invitation),
            InvitedBy = invitedByName,
            TenantId = invitation.TenantId,
            InvitationToken = invitation.InvitationToken
        };
    }

    private static string GetStatusText(UserInvitation invitation)
    {
        if (invitation.IsRevoked)
            return "Revoked";
        
        if (invitation.IsUsed)
            return "Accepted";
        
        if (DateTime.UtcNow > invitation.ExpiresAt)
            return "Expired";
        
        return "Pending";
    }
}
