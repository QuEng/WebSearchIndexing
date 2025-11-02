using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WebSearchIndexing.Modules.Catalog.Domain;

namespace WebSearchIndexing.Modules.Catalog.Infrastructure.Persistence.Configurations;

internal sealed class UrlBatchConfiguration : IEntityTypeConfiguration<UrlBatch>
{
    public void Configure(EntityTypeBuilder<UrlBatch> builder)
    {
        builder.ToTable("UrlBatches");

        builder.HasKey(batch => batch.Id);

        builder.Property(batch => batch.Id)
            .ValueGeneratedNever();

        builder.Property(batch => batch.SiteId)
            .IsRequired();

        builder.Property(batch => batch.CreatedAt)
            .IsRequired();

        builder.Property<Guid>("TenantId")
            .HasColumnName("TenantId")
            .HasDefaultValue(Guid.Empty);

        builder.Ignore(batch => batch.Items);
    }
}
