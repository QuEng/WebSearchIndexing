using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WebSearchIndexing.Modules.Identity.Domain.Repositories;
using WebSearchIndexing.Modules.Identity.Domain.Entities;
using WebSearchIndexing.Modules.Identity.Infrastructure.Persistence;

namespace WebSearchIndexing.Modules.Identity.Infrastructure.Persistence.Repositories;

public class UserTenantRepository : IUserTenantRepository
{
    private readonly IdentityDbContext _context;
    private readonly ILogger<UserTenantRepository> _logger;

    public UserTenantRepository(IdentityDbContext context, ILogger<UserTenantRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<UserTenant?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Set<UserTenant>()
            .Include(ut => ut.User)
            .Include(ut => ut.Tenant)
            .FirstOrDefaultAsync(ut => ut.Id == id, cancellationToken);
    }

    public async Task<UserTenant?> GetByUserAndTenantAsync(Guid userId, Guid tenantId, CancellationToken cancellationToken = default)
    {
        return await _context.Set<UserTenant>()
            .Include(ut => ut.User)
            .Include(ut => ut.Tenant)
            .FirstOrDefaultAsync(ut => ut.UserId == userId && ut.TenantId == tenantId, cancellationToken);
    }

    public async Task<IReadOnlyCollection<UserTenant>> GetByUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting UserTenants for user {UserId} (exact GUID: {UserIdRaw})", userId, userId.ToString());
        
        // First check if there are ANY UserTenant records in the database
        var totalCount = await _context.Set<UserTenant>().CountAsync(cancellationToken);
        _logger.LogDebug("Total UserTenant records in database: {Count}", totalCount);
        
        // Let's also check all UserTenant records to see what we have
        var allUserTenants = await _context.Set<UserTenant>().ToListAsync(cancellationToken);
        _logger.LogDebug("All UserTenant records in database:");
        foreach (var ut in allUserTenants)
        {
            _logger.LogDebug("  - UserTenant: Id={Id}, UserId={UserId}, TenantId={TenantId}, Role={Role}, IsActive={IsActive}", 
                ut.Id, ut.UserId, ut.TenantId, ut.Role, ut.IsActive);
        }
        
        var userTenants = await _context.Set<UserTenant>()
            .Include(ut => ut.Tenant)
            .Where(ut => ut.UserId == userId)
            .OrderByDescending(ut => ut.CreatedAt)
            .ToListAsync(cancellationToken);
        
        _logger.LogInformation("Found {Count} UserTenant records for user {UserId}", userTenants.Count, userId);
        
        // Log each found record
        foreach (var ut in userTenants)
        {
            _logger.LogDebug("UserTenant record: Id={Id}, UserId={UserId}, TenantId={TenantId}, Role={Role}, IsActive={IsActive}, CreatedAt={CreatedAt}", 
                ut.Id, ut.UserId, ut.TenantId, ut.Role, ut.IsActive, ut.CreatedAt);
        }
        
        return userTenants.AsReadOnly();
    }

    public async Task<IReadOnlyCollection<UserTenant>> GetByTenantAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        var userTenants = await _context.Set<UserTenant>()
            .Include(ut => ut.User)
            .Where(ut => ut.TenantId == tenantId)
            .OrderByDescending(ut => ut.CreatedAt)
            .ToListAsync(cancellationToken);
        
        return userTenants.AsReadOnly();
    }

    public async Task<UserTenant> AddAsync(UserTenant userTenant, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Adding UserTenant: Id={Id}, UserId={UserId}, TenantId={TenantId}, Role={Role}, IsActive={IsActive}", 
            userTenant.Id, userTenant.UserId, userTenant.TenantId, userTenant.Role, userTenant.IsActive);
            
        await _context.Set<UserTenant>().AddAsync(userTenant, cancellationToken);
        
        var result = await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("SaveChanges returned {Result} for UserTenant {Id}", result, userTenant.Id);
        
        // Verify the record was actually saved
        var saved = await _context.Set<UserTenant>().FindAsync(new object[] { userTenant.Id }, cancellationToken);
        if (saved != null)
        {
            _logger.LogInformation("UserTenant {Id} verified in database after save", userTenant.Id);
        }
        else
        {
            _logger.LogError("UserTenant {Id} NOT found in database after save!", userTenant.Id);
        }
        
        return userTenant;
    }

    public async Task<UserTenant> UpdateAsync(UserTenant userTenant, CancellationToken cancellationToken = default)
    {
        _context.Set<UserTenant>().Update(userTenant);
        await _context.SaveChangesAsync(cancellationToken);
        return userTenant;
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var userTenant = await _context.Set<UserTenant>().FindAsync(new object[] { id }, cancellationToken);
        if (userTenant != null)
        {
            _context.Set<UserTenant>().Remove(userTenant);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<bool> ExistsAsync(Guid userId, Guid tenantId, CancellationToken cancellationToken = default)
    {
        return await _context.Set<UserTenant>()
            .AnyAsync(ut => ut.UserId == userId && ut.TenantId == tenantId, cancellationToken);
    }
}
