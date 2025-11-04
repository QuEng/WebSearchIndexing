using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WebSearchIndexing.Modules.Catalog.Domain.Entities;

namespace WebSearchIndexing.Modules.Catalog.Infrastructure.Persistence.Configurations;

internal sealed class UrlItemConfiguration : IEntityTypeConfiguration<UrlItem>
{
    public void Configure(EntityTypeBuilder<UrlItem> builder)
    {
        builder.ToTable("UrlRequests");

        builder.HasKey(item => item.Id);

        builder.Property(item => item.Id).ValueGeneratedNever();
        builder.Property(item => item.Url).IsRequired();
        builder.Property(item => item.Type).IsRequired();
        builder.Property(item => item.Priority).IsRequired();
        builder.Property(item => item.Status).IsRequired();
        builder.Property(item => item.AddedAt).IsRequired();
        builder.Property(item => item.ProcessedAt).IsRequired();
        builder.Property(item => item.ServiceAccountId);
        builder.Property(item => item.FailureCount).IsRequired().HasDefaultValue(0);

        builder.Property<Guid>("TenantId")
               .HasColumnName("TenantId")
               .HasDefaultValue(Guid.Empty);

        builder.HasOne(item => item.ServiceAccount)
               .WithMany()
               .HasForeignKey(item => item.ServiceAccountId);

        builder.Ignore(item => item.IsPending);
        builder.Ignore(item => item.IsCompleted);
        builder.Ignore(item => item.IsFailed);
    }
}
