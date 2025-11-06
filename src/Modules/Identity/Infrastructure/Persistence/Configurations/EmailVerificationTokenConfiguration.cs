using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WebSearchIndexing.Modules.Identity.Domain.Entities;

namespace WebSearchIndexing.Modules.Identity.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework configuration for EmailVerificationToken
/// </summary>
internal sealed class EmailVerificationTokenConfiguration : IEntityTypeConfiguration<EmailVerificationToken>
{
    public void Configure(EntityTypeBuilder<EmailVerificationToken> builder)
    {
        builder.ToTable("EmailVerificationTokens", "identity");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.UserId)
            .IsRequired();

        builder.Property(t => t.Email)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(t => t.Token)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(t => t.CreatedAt)
            .IsRequired();

        builder.Property(t => t.ExpiresAt)
            .IsRequired();

        builder.Property(t => t.IsUsed)
            .IsRequired();

        builder.Property(t => t.UsedAt)
            .IsRequired(false);

        // Indexes
        builder.HasIndex(t => t.Token)
            .IsUnique();

        builder.HasIndex(t => t.UserId);

        builder.HasIndex(t => t.ExpiresAt);

        // Relationship with User
        builder.HasOne(t => t.User)
            .WithMany()
            .HasForeignKey(t => t.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
