using Finbuckle.MultiTenant;
using Microsoft.EntityFrameworkCore;
using WebSearchIndexing.BuildingBlocks.Messaging.Outbox;
using WebSearchIndexing.Modules.Identity.Domain.Entities;
using WebSearchIndexing.Modules.Identity.Infrastructure.Persistence.Configurations;

namespace WebSearchIndexing.Modules.Identity.Infrastructure.Persistence;

public sealed class IdentityDbContext : DbContext
{
    private readonly IMultiTenantContextAccessor<TenantInfo> _tenantContextAccessor;

    public IdentityDbContext(
        DbContextOptions<IdentityDbContext> options,
        IMultiTenantContextAccessor<TenantInfo> tenantContextAccessor) : base(options)
    {
        _tenantContextAccessor = tenantContextAccessor;
    }

    internal Guid CurrentTenantId => TryResolveTenantId(out var tenantId)
        ? tenantId
        : Guid.Empty; // For Identity module, we might use a different default

    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Tenant> Tenants { get; set; } = null!;
    public DbSet<Role> Roles { get; set; } = null!;
    public DbSet<UserTenant> UserTenants { get; set; } = null!;
    public DbSet<UserDomain> UserDomains { get; set; } = null!;
    public DbSet<RefreshToken> RefreshTokens { get; set; } = null!;
    public DbSet<UserInvitation> UserInvitations { get; set; } = null!;
    public DbSet<EmailVerificationToken> EmailVerificationTokens { get; set; } = null!;
    public DbSet<PasswordHistory> PasswordHistory { get; set; } = null!;
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);

        base.OnModelCreating(modelBuilder);

        // Apply outbox configuration
        modelBuilder.ApplyConfiguration(new BuildingBlocks.Persistence.Configurations.OutboxMessageConfiguration());

        // Apply entity configurations
        modelBuilder.ApplyConfiguration(new UserConfiguration());
        modelBuilder.ApplyConfiguration(new TenantConfiguration());
        modelBuilder.ApplyConfiguration(new RoleConfiguration());
        modelBuilder.ApplyConfiguration(new UserTenantConfiguration());
        modelBuilder.ApplyConfiguration(new UserDomainConfiguration());
        modelBuilder.ApplyConfiguration(new RefreshTokenConfiguration());
        modelBuilder.ApplyConfiguration(new UserInvitationConfiguration());

        // Set default schema for Identity module
        modelBuilder.HasDefaultSchema("identity");

        // Apply tenant filtering for multi-tenant entities
        modelBuilder.Entity<UserTenant>()
            .HasQueryFilter(entity => entity.TenantId == CurrentTenantId);

        modelBuilder.Entity<OutboxMessage>()
            .HasQueryFilter(entity => entity.TenantId == CurrentTenantId);
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
