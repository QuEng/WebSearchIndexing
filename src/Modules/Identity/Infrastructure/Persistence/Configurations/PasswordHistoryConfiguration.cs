using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WebSearchIndexing.Modules.Identity.Domain.Entities;

namespace WebSearchIndexing.Modules.Identity.Infrastructure.Persistence.Configurations;

public sealed class PasswordHistoryConfiguration : IEntityTypeConfiguration<PasswordHistory>
{
    public void Configure(EntityTypeBuilder<PasswordHistory> builder)
    {
        builder.ToTable("PasswordHistory", "identity");

        builder.HasKey(ph => ph.Id);

        builder.Property(ph => ph.UserId)
            .IsRequired();

        builder.Property(ph => ph.PasswordHash)
            .IsRequired();

        builder.Property(ph => ph.CreatedAt)
            .IsRequired();

        // Relationship with User
        builder.HasOne(ph => ph.User)
            .WithMany(u => u.PasswordHistory)
            .HasForeignKey(ph => ph.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(ph => ph.UserId);
        builder.HasIndex(ph => ph.CreatedAt);
    }
}
