using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WebSearchIndexing.BuildingBlocks.Messaging.Outbox;

namespace WebSearchIndexing.BuildingBlocks.Persistence.Configurations;

/// <summary>
/// Entity Framework configuration for OutboxMessage
/// </summary>
public class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("outbox_messages");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .IsRequired();

        builder.Property(x => x.TenantId)
            .HasColumnName("tenant_id")
            .IsRequired();

        builder.Property(x => x.Type)
            .HasColumnName("type")
            .HasMaxLength(512)
            .IsRequired();

        builder.Property(x => x.Data)
            .HasColumnName("data")
            .IsRequired();

        builder.Property(x => x.OccurredOn)
            .HasColumnName("occurred_on")
            .IsRequired();

        builder.Property(x => x.ProcessedOn)
            .HasColumnName("processed_on");

        builder.Property(x => x.Status)
            .HasColumnName("status")
            .HasConversion<int>()
            .IsRequired();

        builder.Property(x => x.Error)
            .HasColumnName("error")
            .HasMaxLength(2048);

        builder.Property(x => x.RetryCount)
            .HasColumnName("retry_count")
            .IsRequired();

        // Indexes for performance
        builder.HasIndex(x => new { x.TenantId, x.Status, x.OccurredOn })
            .HasDatabaseName("ix_outbox_messages_tenant_status_occurred");

        builder.HasIndex(x => x.Status)
            .HasDatabaseName("ix_outbox_messages_status");

        builder.HasIndex(x => x.OccurredOn)
            .HasDatabaseName("ix_outbox_messages_occurred_on");
    }
}
