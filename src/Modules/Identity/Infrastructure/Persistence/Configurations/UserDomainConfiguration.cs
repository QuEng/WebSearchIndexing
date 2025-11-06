using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WebSearchIndexing.Modules.Identity.Domain.Entities;

namespace WebSearchIndexing.Modules.Identity.Infrastructure.Persistence.Configurations;

public class UserDomainConfiguration : IEntityTypeConfiguration<UserDomain>
{
    public void Configure(EntityTypeBuilder<UserDomain> builder)
    {
        builder.ToTable("UserDomains");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedNever()
            .IsRequired();

        builder.Property(x => x.UserId)
            .IsRequired();

        builder.Property(x => x.DomainId)
            .IsRequired();

        builder.Property(x => x.Role)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.DeactivatedAt);

        builder.Property(x => x.GrantedBy);

        // Composite unique index - one user can have one role per domain
        builder.HasIndex(x => new { x.UserId, x.DomainId })
            .IsUnique();

        // Foreign key relationships
        builder.HasOne(x => x.User)
            .WithMany(x => x.UserDomains)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Note: DomainId is a foreign key to a domain in another module
        // We don't define the navigation property here due to cross-module boundaries
    }
}
