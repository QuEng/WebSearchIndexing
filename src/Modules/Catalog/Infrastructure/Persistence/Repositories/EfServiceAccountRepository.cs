using Microsoft.EntityFrameworkCore;
using WebSearchIndexing.BuildingBlocks.Messaging;
using WebSearchIndexing.Modules.Catalog.Application.Abstractions;
using WebSearchIndexing.Modules.Catalog.Application.IntegrationEvents;
using WebSearchIndexing.Modules.Catalog.Domain.Entities;

namespace WebSearchIndexing.Modules.Catalog.Infrastructure.Persistence.Repositories;

internal sealed class EfServiceAccountRepository : IServiceAccountRepository
{
    private readonly IDbContextFactory<CatalogDbContext> _contextFactory;
    private readonly IUrlRequestRepository _urlRequestRepository;
    private readonly IIntegrationEventPublisher _eventPublisher;

    public EfServiceAccountRepository(
        IDbContextFactory<CatalogDbContext> contextFactory,
        IUrlRequestRepository urlRequestRepository,
        IIntegrationEventPublisher eventPublisher)
    {
        _contextFactory = contextFactory;
        _urlRequestRepository = urlRequestRepository;
        _eventPublisher = eventPublisher;
    }

    public async Task<ServiceAccount> AddAsync(ServiceAccount entity, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entity);

        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        SetTenantId(context, entity);

        await context.ServiceAccounts.AddAsync(entity, cancellationToken);
        
        // Publish integration event
        var tenantId = (context as CatalogDbContext)?.CurrentTenantId.ToString() ?? string.Empty;
        var integrationEvent = new ServiceAccountCreatedEvent(
            tenantId, 
            entity.Id, 
            entity.ProjectId, 
            entity.QuotaLimitPerDay);
        
        await _eventPublisher.PublishAsync(integrationEvent, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        return entity;
    }

    public async Task<bool> UpdateAsync(ServiceAccount entity, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entity);

        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        SetTenantId(context, entity);

        context.ServiceAccounts.Update(entity);
        
        // Publish integration event
        var tenantId = (context as CatalogDbContext)?.CurrentTenantId.ToString() ?? string.Empty;
        var integrationEvent = new ServiceAccountUpdatedEvent(
            tenantId, 
            entity.Id, 
            entity.ProjectId, 
            entity.QuotaLimitPerDay);
        
        await _eventPublisher.PublishAsync(integrationEvent, cancellationToken);
        var changes = await context.SaveChangesAsync(cancellationToken);

        return changes > 0;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

        var entity = await context.ServiceAccounts
            .FirstOrDefaultAsync(account => account.Id == id, cancellationToken);

        if (entity is null)
        {
            return false;
        }

        entity.MarkDeleted();
        context.ServiceAccounts.Update(entity);

        // Publish integration event
        var tenantId = (context as CatalogDbContext)?.CurrentTenantId.ToString() ?? string.Empty;
        var integrationEvent = new ServiceAccountDeletedEvent(tenantId, entity.Id);
        
        await _eventPublisher.PublishAsync(integrationEvent, cancellationToken);
        var changes = await context.SaveChangesAsync(cancellationToken);
        return changes > 0;
    }

    public async Task<ServiceAccount?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        return await context.ServiceAccounts
            .AsNoTracking()
            .FirstOrDefaultAsync(account => account.Id == id && account.DeletedAt == null, cancellationToken);
    }

    public async Task<List<ServiceAccount>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

        var accounts = await context.ServiceAccounts
            .AsNoTracking()
            .Where(account => account.DeletedAt == null)
            .OrderByDescending(account => account.CreatedAt)
            .ToListAsync(cancellationToken);

        foreach (var account in accounts)
        {
            var usedToday = await _urlRequestRepository
                .GetRequestsCountAsync(
                    TimeSpan.FromDays(1),
                    account.Id,
                    requestStatus: UrlItemStatus.Completed,
                    cancellationToken: cancellationToken);
            account.LoadQuotaUsage((uint)usedToday);
        }

        return accounts;
    }

    public async Task<bool> EntityExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

        return await context.ServiceAccounts
            .AnyAsync(account => account.Id == id && account.DeletedAt == null, cancellationToken);
    }

    public async Task<bool> EntityExistByProjectIdAsync(string projectId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(projectId);

        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

        return await context.ServiceAccounts
            .AnyAsync(account =>
                account.ProjectId == projectId && account.DeletedAt == null,
                cancellationToken);
    }

    public async Task<int> GetCountAsync(CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        return await context.ServiceAccounts
            .CountAsync(account => account.DeletedAt == null, cancellationToken);
    }

    public async Task<int> GetQuotaByAllAsync(CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        return (int)await context.ServiceAccounts
            .Where(account => account.DeletedAt == null)
            .SumAsync(account => account.QuotaLimitPerDay, cancellationToken);
    }

    public async Task<int> GetQuotaAvailableTodayAsync(CancellationToken cancellationToken = default)
    {
        var accounts = await GetAllAsync(cancellationToken);
        return accounts.Sum(account => (int)account.RemainingQuota);
    }

    public async Task<ServiceAccount?> GetWithAvailableLimitAsync(CancellationToken cancellationToken = default)
    {
        var accounts = await GetAllAsync(cancellationToken);
        return accounts.FirstOrDefault(account => account.CanHandle(1));
    }

    private static void SetTenantId(DbContext context, ServiceAccount entity)
    {
        var tenantId = (context as CatalogDbContext)?.CurrentTenantId ?? Guid.Empty;
        context.Entry(entity).Property<Guid>("TenantId").CurrentValue = tenantId;
    }
}
