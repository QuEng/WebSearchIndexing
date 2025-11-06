using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WebSearchIndexing.Modules.Identity.Domain.Entities;

namespace WebSearchIndexing.Modules.Identity.Infrastructure.Persistence.Configurations;

public class UserInvitationConfiguration : IEntityTypeConfiguration<UserInvitation>
{
    public void Configure(EntityTypeBuilder<UserInvitation> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("user_invitations");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(x => x.Email)
            .HasColumnName("email")
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(x => x.InvitedByUserId)
            .HasColumnName("invited_by_user_id")
            .IsRequired();

        builder.Property(x => x.TenantId)
            .HasColumnName("tenant_id");

        builder.Property(x => x.DomainId)
            .HasColumnName("domain_id");

        builder.Property(x => x.Role)
            .HasColumnName("role")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.InvitationToken)
            .HasColumnName("invitation_token")
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(x => x.ExpiresAt)
            .HasColumnName("expires_at")
            .IsRequired();

        builder.Property(x => x.IsUsed)
            .HasColumnName("is_used")
            .IsRequired();

        builder.Property(x => x.UsedAt)
            .HasColumnName("used_at");

        builder.Property(x => x.AcceptedByUserId)
            .HasColumnName("accepted_by_user_id");

        builder.Property(x => x.IsRevoked)
            .HasColumnName("is_revoked")
            .IsRequired();

        builder.Property(x => x.RevokedAt)
            .HasColumnName("revoked_at");

        // Indexes
        builder.HasIndex(x => x.Email)
            .HasDatabaseName("ix_user_invitations_email");

        builder.HasIndex(x => x.InvitationToken)
            .IsUnique()
            .HasDatabaseName("ux_user_invitations_token");

        builder.HasIndex(x => x.TenantId)
            .HasDatabaseName("ix_user_invitations_tenant_id");

        builder.HasIndex(x => x.DomainId)
            .HasDatabaseName("ix_user_invitations_domain_id");

        builder.HasIndex(x => new { x.Email, x.IsUsed, x.IsRevoked, x.ExpiresAt })
            .HasDatabaseName("ix_user_invitations_pending");
    }
}
