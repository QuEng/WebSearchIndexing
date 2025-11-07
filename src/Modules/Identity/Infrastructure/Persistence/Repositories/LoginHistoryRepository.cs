using Microsoft.EntityFrameworkCore;
using WebSearchIndexing.Modules.Identity.Domain.Entities;
using WebSearchIndexing.Modules.Identity.Domain.Repositories;

namespace WebSearchIndexing.Modules.Identity.Infrastructure.Persistence.Repositories;

internal sealed class LoginHistoryRepository : ILoginHistoryRepository
{
    private readonly IdentityDbContext _context;

    public LoginHistoryRepository(IdentityDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<LoginHistory> AddAsync(LoginHistory loginHistory, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(loginHistory);

        _context.LoginHistories.Add(loginHistory);
        await _context.SaveChangesAsync(cancellationToken);
        return loginHistory;
    }

    public async Task<LoginHistory?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.LoginHistories
            .Include(lh => lh.User)
            .FirstOrDefaultAsync(lh => lh.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyCollection<LoginHistory>> GetByUserIdAsync(
        Guid userId, 
        int limit = 50, 
        CancellationToken cancellationToken = default)
    {
        return await _context.LoginHistories
            .Where(lh => lh.UserId == userId)
            .OrderByDescending(lh => lh.LoginAt)
            .Take(limit)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<LoginHistory>> GetRecentByUserIdAsync(
        Guid userId, 
        int days = 30,
        int limit = 50, 
        CancellationToken cancellationToken = default)
    {
        var cutoffDate = DateTime.UtcNow.AddDays(-days);
        
        return await _context.LoginHistories
            .Where(lh => lh.UserId == userId && lh.LoginAt >= cutoffDate)
            .OrderByDescending(lh => lh.LoginAt)
            .Take(limit)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<LoginHistory>> GetFailedAttemptsAsync(
        string ipAddress, 
        TimeSpan timeWindow,
        CancellationToken cancellationToken = default)
    {
        var cutoffDate = DateTime.UtcNow.Subtract(timeWindow);
        
        return await _context.LoginHistories
            .Where(lh => lh.IpAddress == ipAddress 
                        && !lh.IsSuccessful 
                        && lh.LoginAt >= cutoffDate)
            .OrderByDescending(lh => lh.LoginAt)
            .ToListAsync(cancellationToken);
    }

    public async Task DeleteOldRecordsAsync(DateTime beforeDate, CancellationToken cancellationToken = default)
    {
        await _context.LoginHistories
            .Where(lh => lh.LoginAt < beforeDate)
            .ExecuteDeleteAsync(cancellationToken);
    }
}
