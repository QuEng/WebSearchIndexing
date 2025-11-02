using Finbuckle.MultiTenant;
using Microsoft.EntityFrameworkCore;
using WebSearchIndexing.Modules.Core.Domain;
using CoreSettings = WebSearchIndexing.Modules.Core.Domain.Settings;

namespace WebSearchIndexing.Modules.Core.Infrastructure;

public sealed class CoreDbContext : DbContext
{
    private readonly IMultiTenantContextAccessor<TenantInfo> _tenantContextAccessor;

    public CoreDbContext(
        DbContextOptions<CoreDbContext> options,
        IMultiTenantContextAccessor<TenantInfo> tenantContextAccessor) : base(options)
    {
        _tenantContextAccessor = tenantContextAccessor;
    }

    internal Guid CurrentTenantId => TryResolveTenantId(out var tenantId)
        ? tenantId
        : CoreSettings.DefaultTenantId;

    public DbSet<Settings> Settings => Set<Settings>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);

        base.OnModelCreating(modelBuilder);

        var settings = modelBuilder.Entity<Settings>();
        settings.ToTable("Settings");
        settings.HasKey(s => s.Id);

        settings.Property(s => s.Id)
            .HasColumnName("id");

        settings.Property(s => s.TenantId)
            .HasColumnName("tenant_id")
            .HasDefaultValue(CoreSettings.DefaultTenantId)
            .IsRequired();

        settings.Property(s => s.Key)
            .HasColumnName("key")
            .HasMaxLength(128)
            .IsRequired();

        settings.Property(s => s.RequestsPerDay)
            .HasColumnName("requests_per_day")
            .IsRequired();

        settings.Property(s => s.IsEnabled)
            .HasColumnName("is_enabled")
            .IsRequired();

        settings.HasIndex(s => new { s.TenantId, s.Key })
            .IsUnique()
            .HasDatabaseName("ux_settings_tenant_key");

        settings.HasQueryFilter(entity => entity.TenantId == CurrentTenantId);
    }

    private bool TryResolveTenantId(out Guid tenantId)
    {
        var tenantIdValue = _tenantContextAccessor.MultiTenantContext?.TenantInfo?.Id;
        if (!string.IsNullOrWhiteSpace(tenantIdValue) && Guid.TryParse(tenantIdValue, out tenantId))
        {
            return true;
        }

        tenantId = CoreSettings.DefaultTenantId;
        return false;
    }
}
