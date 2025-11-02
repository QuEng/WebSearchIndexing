using Finbuckle.MultiTenant;
using Microsoft.EntityFrameworkCore;
using WebSearchIndexing.Modules.Catalog.Domain;

namespace WebSearchIndexing.Modules.Catalog.Infrastructure.Persistence;

public sealed class CatalogDbContext : DbContext
{
    private readonly IMultiTenantContextAccessor<TenantInfo> _tenantContextAccessor;

    public CatalogDbContext(
        DbContextOptions<CatalogDbContext> options,
        IMultiTenantContextAccessor<TenantInfo> tenantContextAccessor) : base(options)
    {
        _tenantContextAccessor = tenantContextAccessor;
    }

    internal Guid CurrentTenantId => TryResolveTenantId(out var tenantId)
        ? tenantId
        : Guid.Empty;

    public DbSet<ServiceAccount> ServiceAccounts => Set<ServiceAccount>();
    public DbSet<UrlItem> UrlItems => Set<UrlItem>();
    public DbSet<Site> Sites => Set<Site>();
    public DbSet<UrlBatch> UrlBatches => Set<UrlBatch>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);

        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CatalogDbContext).Assembly);

        modelBuilder.Entity<ServiceAccount>()
            .HasQueryFilter(entity => EF.Property<Guid>(entity, "TenantId") == CurrentTenantId);

        modelBuilder.Entity<UrlItem>()
            .HasQueryFilter(entity => EF.Property<Guid>(entity, "TenantId") == CurrentTenantId);

        modelBuilder.Entity<Site>()
            .HasQueryFilter(entity => EF.Property<Guid>(entity, "TenantId") == CurrentTenantId);

        modelBuilder.Entity<UrlBatch>()
            .HasQueryFilter(entity => EF.Property<Guid>(entity, "TenantId") == CurrentTenantId);
    }

    private bool TryResolveTenantId(out Guid tenantId)
    {
        var tenantIdValue = _tenantContextAccessor.MultiTenantContext?.TenantInfo?.Id;
        if (!string.IsNullOrWhiteSpace(tenantIdValue) && Guid.TryParse(tenantIdValue, out tenantId))
        {
            return true;
        }

        tenantId = Guid.Empty;
        return false;
    }
}
