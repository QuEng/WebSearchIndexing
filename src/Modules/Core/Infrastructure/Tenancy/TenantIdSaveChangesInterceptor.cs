using Finbuckle.MultiTenant;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using CoreSettings = WebSearchIndexing.Modules.Core.Domain.Entities.Settings;

namespace WebSearchIndexing.Modules.Core.Infrastructure.Tenancy;

internal sealed class TenantIdSaveChangesInterceptor : SaveChangesInterceptor
{
    private readonly IMultiTenantContextAccessor<TenantInfo> _tenantContextAccessor;
    private readonly ILogger<TenantIdSaveChangesInterceptor> _logger;

    public TenantIdSaveChangesInterceptor(
    IMultiTenantContextAccessor<TenantInfo> tenantContextAccessor,
    ILogger<TenantIdSaveChangesInterceptor> logger)
    {
        _tenantContextAccessor = tenantContextAccessor;
        _logger = logger;
    }

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        ApplyTenantId(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        ApplyTenantId(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void ApplyTenantId(DbContext? context)
    {
        if (context is null) return;
        var tenantId = ResolveTenantId();
        foreach (var entry in context.ChangeTracker.Entries())
        {
            if (entry.State != EntityState.Added) continue;
            var prop = entry.Properties.FirstOrDefault(p => p.Metadata.Name == nameof(CoreSettings.TenantId));
            if (prop is null) continue;
            if (prop.CurrentValue is Guid guid && guid != Guid.Empty) continue;
            try
            {
                prop.CurrentValue = tenantId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to assign TenantId for entity {EntityType}", entry.Metadata.Name);
                throw;
            }
        }
    }

    private Guid ResolveTenantId()
    {
        var tenantIdValue = _tenantContextAccessor.MultiTenantContext?.TenantInfo?.Id;
        return Guid.TryParse(tenantIdValue, out var id) ? id : CoreSettings.DefaultTenantId;
    }
}
