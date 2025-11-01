using Microsoft.EntityFrameworkCore;
using WebSearchIndexing.Domain.Entities;
using WebSearchIndexing.Modules.Catalog.Domain;

namespace WebSearchIndexing.Data;

public class IndexingDbContext(DbContextOptions<IndexingDbContext> options) : DbContext(options)
{
    public DbSet<ServiceAccount> ServiceAccounts => Set<ServiceAccount>();
    public DbSet<UrlItem> UrlItems => Set<UrlItem>();
    public DbSet<Setting> Settings => Set<Setting>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        var serviceAccountBuilder = modelBuilder.Entity<ServiceAccount>();
        serviceAccountBuilder.ToTable("ServiceAccounts");
        serviceAccountBuilder.Property(account => account.ProjectId).IsRequired();
        serviceAccountBuilder.Property(account => account.CredentialsJson)
            .HasColumnName("Json")
            .IsRequired();
        serviceAccountBuilder.Property(account => account.QuotaLimitPerDay).IsRequired();
        serviceAccountBuilder.Ignore(account => account.QuotaUsedInPeriod);

        var urlItemBuilder = modelBuilder.Entity<UrlItem>();
        urlItemBuilder.ToTable("UrlRequests");
        urlItemBuilder.Property(item => item.Url).IsRequired();
        urlItemBuilder.Property(item => item.Type).IsRequired();
        urlItemBuilder.Property(item => item.Priority).IsRequired();
        urlItemBuilder.Property(item => item.Status).IsRequired();
        urlItemBuilder.Property(item => item.AddedAt).IsRequired();
        urlItemBuilder.Property(item => item.ProcessedAt).IsRequired();
        urlItemBuilder.Property(item => item.ServiceAccountId);
        urlItemBuilder.HasOne(item => item.ServiceAccount)
            .WithMany()
            .HasForeignKey(item => item.ServiceAccountId);
    }
}
