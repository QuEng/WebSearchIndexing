using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WebSearchIndexing.Modules.Identity.Domain.Entities;

namespace WebSearchIndexing.Modules.Identity.Infrastructure.Persistence.Configurations;

internal sealed class LoginHistoryConfiguration : IEntityTypeConfiguration<LoginHistory>
{
    public void Configure(EntityTypeBuilder<LoginHistory> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("login_histories", "identity");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedNever()
            .HasColumnName("id");

        builder.Property(x => x.UserId)
            .IsRequired()
            .HasColumnName("user_id");

        builder.Property(x => x.IpAddress)
            .IsRequired()
            .HasMaxLength(45) // IPv6 max length
            .HasColumnName("ip_address");

        builder.Property(x => x.UserAgent)
            .IsRequired()
            .HasMaxLength(1000)
            .HasColumnName("user_agent");

        builder.Property(x => x.Location)
            .HasMaxLength(200)
            .HasColumnName("location");

        builder.Property(x => x.LoginAt)
            .IsRequired()
            .HasColumnName("login_at");

        builder.Property(x => x.IsSuccessful)
            .IsRequired()
            .HasColumnName("is_successful");

        builder.Property(x => x.FailureReason)
            .HasMaxLength(500)
            .HasColumnName("failure_reason");

        builder.Property(x => x.DeviceInfo)
            .HasMaxLength(500)
            .HasColumnName("device_info");

        builder.Property(x => x.TenantId)
            .HasColumnName("tenant_id");

        // Indexes
        builder.HasIndex(x => x.UserId)
            .HasDatabaseName("ix_login_histories_user_id");

        builder.HasIndex(x => x.LoginAt)
            .HasDatabaseName("ix_login_histories_login_at");

        builder.HasIndex(x => new { x.IpAddress, x.LoginAt })
            .HasDatabaseName("ix_login_histories_ip_login_at");

        builder.HasIndex(x => new { x.UserId, x.LoginAt })
            .HasDatabaseName("ix_login_histories_user_login_at");

        // Relationships
        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
