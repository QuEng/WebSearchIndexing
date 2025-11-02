using Microsoft.EntityFrameworkCore;
using WebSearchIndexing.BuildingBlocks.Messaging.Outbox;

namespace WebSearchIndexing.BuildingBlocks.Persistence.Repositories;

/// <summary>
/// Entity Framework implementation of the outbox repository
/// </summary>
public class EfOutboxRepository<TDbContext> : IOutboxRepository where TDbContext : DbContext
{
    private readonly TDbContext _dbContext;

    public EfOutboxRepository(TDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public async Task AddAsync(OutboxMessage message, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(message);

        await _dbContext.Set<OutboxMessage>().AddAsync(message, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<IEnumerable<OutboxMessage>> GetPendingMessagesAsync(
        int batchSize = 100, 
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Set<OutboxMessage>()
            .Where(m => m.Status == OutboxMessageStatus.Pending)
            .OrderBy(m => m.OccurredOn)
            .Take(batchSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<OutboxMessage>> GetPendingMessagesAsync(
        string tenantId,
        int batchSize = 100,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(tenantId);

        var tenantGuid = Guid.Parse(tenantId);
        return await _dbContext.Set<OutboxMessage>()
            .Where(m => m.Status == OutboxMessageStatus.Pending && m.TenantId == tenantGuid)
            .OrderBy(m => m.OccurredOn)
            .Take(batchSize)
            .ToListAsync(cancellationToken);
    }

    public async Task UpdateAsync(OutboxMessage message, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(message);

        _dbContext.Set<OutboxMessage>().Update(message);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task CleanupProcessedMessagesAsync(
        DateTime beforeDate,
        CancellationToken cancellationToken = default)
    {
        var messagesToDelete = await _dbContext.Set<OutboxMessage>()
            .Where(m => m.Status == OutboxMessageStatus.Processed && m.ProcessedOn < beforeDate)
            .ToListAsync(cancellationToken);

        _dbContext.Set<OutboxMessage>().RemoveRange(messagesToDelete);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
