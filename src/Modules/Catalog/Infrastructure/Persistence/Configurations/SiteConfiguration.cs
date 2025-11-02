using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WebSearchIndexing.Modules.Catalog.Domain;

namespace WebSearchIndexing.Modules.Catalog.Infrastructure.Persistence.Configurations;

internal sealed class SiteConfiguration : IEntityTypeConfiguration<Site>
{
    public void Configure(EntityTypeBuilder<Site> builder)
    {
        builder.ToTable("Sites");

        builder.HasKey(site => site.Id);

        builder.Property(site => site.Id)
            .ValueGeneratedNever();

        builder.Property(site => site.Host)
            .HasMaxLength(512)
            .IsRequired();

        builder.Property(site => site.DisplayName)
            .HasMaxLength(512);

        builder.Property(site => site.CreatedAt)
            .IsRequired();

        builder.Property<Guid>("TenantId")
            .HasColumnName("TenantId")
            .HasDefaultValue(Guid.Empty);

        builder.Ignore(site => site.ServiceAccounts);
        builder.Ignore(site => site.UrlBatches);
    }
}
