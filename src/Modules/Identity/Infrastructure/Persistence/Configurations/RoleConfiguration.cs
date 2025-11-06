using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WebSearchIndexing.Modules.Identity.Domain.Constants;
using WebSearchIndexing.Modules.Identity.Domain.Entities;

namespace WebSearchIndexing.Modules.Identity.Infrastructure.Persistence.Configurations;

public class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.ToTable("Roles");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedNever()
            .IsRequired();

        builder.Property(x => x.Name)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.Type)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(x => x.Permissions)
            .HasMaxLength(2000);

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        // Name and Type combination must be unique
        builder.HasIndex(x => new { x.Name, x.Type })
            .IsUnique();

        // Seed default roles
        var seedDate = new DateTime(2024, 11, 4, 0, 0, 0, DateTimeKind.Utc);
        
        builder.HasData(
            new Role(
                Guid.Parse("11111111-1111-1111-1111-111111111111"),
                Roles.GlobalAdmin,
                RoleType.Global,
                Permissions.Join(Permissions.Groups.GlobalAdminPermissions),
                seedDate,
                true),
            new Role(
                Guid.Parse("22222222-2222-2222-2222-222222222222"),
                Roles.TenantAdmin,
                RoleType.Tenant,
                Permissions.Join(Permissions.Groups.TenantAdminPermissions),
                seedDate,
                true),
            new Role(
                Guid.Parse("33333333-3333-3333-3333-333333333333"),
                Roles.TenantUser,
                RoleType.Tenant,
                Permissions.Join(Permissions.Groups.TenantUserPermissions),
                seedDate,
                true),
            new Role(
                Guid.Parse("44444444-4444-4444-4444-444444444444"),
                Roles.DomainAdmin,
                RoleType.Domain,
                Permissions.Join(Permissions.Groups.DomainAdminPermissions),
                seedDate,
                true),
            new Role(
                Guid.Parse("55555555-5555-5555-5555-555555555555"),
                Roles.DomainUser,
                RoleType.Domain,
                Permissions.Join(Permissions.Groups.DomainUserPermissions),
                seedDate,
                true)
        );
    }
}
