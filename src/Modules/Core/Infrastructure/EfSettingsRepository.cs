using Finbuckle.MultiTenant;
using Microsoft.EntityFrameworkCore;
using WebSearchIndexing.BuildingBlocks.Messaging;
using WebSearchIndexing.Modules.Core.Application;
using WebSearchIndexing.Modules.Core.Application.IntegrationEvents;
using WebSearchIndexing.Modules.Core.Domain;
using CoreSettings = WebSearchIndexing.Modules.Core.Domain.Settings;

namespace WebSearchIndexing.Modules.Core.Infrastructure;

public sealed class EfSettingsRepository : ISettingsRepository
{
    private readonly IDbContextFactory<CoreDbContext> _dbContextFactory;
    private readonly IMultiTenantContextAccessor<TenantInfo> _tenantContextAccessor;
    private readonly IIntegrationEventPublisher _eventPublisher;

    public EfSettingsRepository(
        IDbContextFactory<CoreDbContext> dbContextFactory,
        IMultiTenantContextAccessor<TenantInfo> tenantContextAccessor,
        IIntegrationEventPublisher eventPublisher)
    {
        _dbContextFactory = dbContextFactory;
        _tenantContextAccessor = tenantContextAccessor;
        _eventPublisher = eventPublisher;
    }

    public async Task<Settings> GetAsync(CancellationToken cancellationToken = default)
    {
        var tenantId = ResolveTenantId();

        await using var context = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

        var settings = await context.Settings
            .AsNoTracking()
            .FirstOrDefaultAsync(
                s => s.TenantId == tenantId && s.Key == Settings.DefaultKey,
                cancellationToken);

        if (settings is not null)
        {
            return settings;
        }

        settings = Settings.CreateDefault(tenantId);
        context.Settings.Add(settings);
        await context.SaveChangesAsync(cancellationToken);

        return settings;
    }

    public async Task<bool> UpdateAsync(Settings settings, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(settings);

        await using var context = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
        context.Settings.Update(settings);
        
        // Publish integration event
        var tenantId = ResolveTenantId().ToString();
        var integrationEvent = new SettingsChangedEvent(
            tenantId, 
            settings.Id, 
            settings.Key, 
            settings.RequestsPerDay, 
            settings.IsEnabled);
        
        await _eventPublisher.PublishAsync(integrationEvent, cancellationToken);
        var changes = await context.SaveChangesAsync(cancellationToken);

        return changes > 0;
    }

    private Guid ResolveTenantId()
    {
        var tenantIdValue = _tenantContextAccessor.MultiTenantContext?.TenantInfo?.Id;
        if (!string.IsNullOrWhiteSpace(tenantIdValue) && Guid.TryParse(tenantIdValue, out var tenantId))
        {
            return tenantId;
        }

        return CoreSettings.DefaultTenantId;
    }
}
