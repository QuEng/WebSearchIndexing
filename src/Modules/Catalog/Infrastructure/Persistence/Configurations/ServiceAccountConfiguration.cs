using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WebSearchIndexing.Modules.Catalog.Domain.Entities;

namespace WebSearchIndexing.Modules.Catalog.Infrastructure.Persistence.Configurations;

internal sealed class ServiceAccountConfiguration : IEntityTypeConfiguration<ServiceAccount>
{
    public void Configure(EntityTypeBuilder<ServiceAccount> builder)
    {
        builder.ToTable("ServiceAccounts");

        builder.HasKey(account => account.Id);

        builder.Property(account => account.Id).ValueGeneratedNever();
        builder.Property(account => account.ProjectId).IsRequired();
        builder.Property(account => account.CredentialsJson)
               .IsRequired()
               .HasColumnName("Json");
        builder.Property(account => account.QuotaLimitPerDay).IsRequired();
        builder.Property(account => account.CreatedAt).IsRequired();
        builder.Property(account => account.DeletedAt);
        builder.Ignore(account => account.QuotaUsedInPeriod);
        builder.Ignore(account => account.RemainingQuota);
        builder.Ignore(account => account.IsDeleted);

        builder.Property<Guid>("TenantId")
               .HasColumnName("TenantId")
               .HasDefaultValue(Guid.Empty);
    }
}
