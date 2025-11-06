using WebSearchIndexing.Modules.Identity.Domain.Entities;

namespace WebSearchIndexing.Modules.Identity.Application.UserManagement.DTOs;

public sealed record UserInvitationDto(
    Guid Id,
    string Email,
    Guid InvitedByUserId,
    Guid? TenantId,
    Guid? DomainId,
    string Role,
    DateTime CreatedAt,
    DateTime ExpiresAt,
    bool IsUsed,
    DateTime? UsedAt,
    bool IsRevoked,
    DateTime? RevokedAt)
{
    public static UserInvitationDto FromDomain(UserInvitation invitation)
    {
        ArgumentNullException.ThrowIfNull(invitation);

        return new UserInvitationDto(
            invitation.Id,
            invitation.Email,
            invitation.InvitedByUserId,
            invitation.TenantId,
            invitation.DomainId,
            invitation.Role,
            invitation.CreatedAt,
            invitation.ExpiresAt,
            invitation.IsUsed,
            invitation.UsedAt,
            invitation.IsRevoked,
            invitation.RevokedAt);
    }

    public bool IsValid => !IsUsed && !IsRevoked && DateTime.UtcNow <= ExpiresAt;
    public string Status => IsRevoked ? "Revoked" : IsUsed ? "Used" : DateTime.UtcNow > ExpiresAt ? "Expired" : "Pending";
}
