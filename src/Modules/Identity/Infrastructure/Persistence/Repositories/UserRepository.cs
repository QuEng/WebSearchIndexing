using Microsoft.EntityFrameworkCore;
using WebSearchIndexing.Modules.Identity.Domain.Repositories;
using WebSearchIndexing.Modules.Identity.Domain.Entities;
using WebSearchIndexing.Modules.Identity.Infrastructure.Persistence;

namespace WebSearchIndexing.Modules.Identity.Infrastructure.Persistence.Repositories;

public class UserRepository : IUserRepository
{
    private readonly IdentityDbContext _context;

    public UserRepository(IdentityDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .Include(x => x.UserTenants)
            .Include(x => x.UserDomains)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .Include(x => x.UserTenants)
            .Include(x => x.UserDomains)
            .FirstOrDefaultAsync(x => x.Email == email, cancellationToken);
    }

    public async Task<IReadOnlyCollection<User>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var users = await _context.Users
            .Include(x => x.UserTenants)
            .Include(x => x.UserDomains)
            .Where(x => x.IsActive)
            .ToListAsync(cancellationToken);
        
        return users.AsReadOnly();
    }

    public async Task<User> AddAsync(User user, CancellationToken cancellationToken = default)
    {
        await _context.Users.AddAsync(user, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return user;
    }

    public async Task<User> UpdateAsync(User user, CancellationToken cancellationToken = default)
    {
        var entry = _context.Entry(user);
        if (entry.State == EntityState.Detached)
        {
            _context.Users.Update(user);
        }
        else
        {
            // Entity is already being tracked, just save changes
            entry.State = EntityState.Modified;
        }
        
        await _context.SaveChangesAsync(cancellationToken);
        return user;
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var user = await _context.Users.FindAsync(new object[] { id }, cancellationToken);
        if (user != null)
        {
            _context.Users.Remove(user);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .AnyAsync(x => x.Email == email, cancellationToken);
    }

    public async Task<IEnumerable<User>> GetByTenantAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        var users = await _context.Users
            .Include(x => x.UserTenants)
            .Include(x => x.UserDomains)
            .Where(x => x.UserTenants.Any(ut => ut.TenantId == tenantId && ut.IsActive) && x.IsActive)
            .ToListAsync(cancellationToken);
        
        return users;
    }

    public async Task<IReadOnlyCollection<User>> GetByDomainAsync(Guid domainId, CancellationToken cancellationToken = default)
    {
        var users = await _context.Users
            .Include(x => x.UserTenants)
            .Include(x => x.UserDomains)
            .Where(x => x.UserDomains.Any(ud => ud.DomainId == domainId && ud.IsActive) && x.IsActive)
            .ToListAsync(cancellationToken);
        
        return users.AsReadOnly();
    }

    public async Task<IReadOnlyCollection<string>> GetUserGlobalRolesAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        // Global roles are stored as UserTenant entries with TenantId = Guid.Empty
        var roles = await _context.Set<UserTenant>()
            .Where(ut => ut.UserId == userId && ut.TenantId == Guid.Empty && ut.IsActive)
            .Select(ut => ut.Role)
            .ToListAsync(cancellationToken);
        
        return roles.AsReadOnly();
    }

    public async Task<(IEnumerable<User> Users, int TotalCount)> GetPagedAsync(
        int page, 
        int pageSize, 
        string? searchTerm = null, 
        CancellationToken cancellationToken = default)
    {
        var query = _context.Users
            .Include(x => x.UserTenants)
            .Include(x => x.UserDomains)
            .Where(x => x.IsActive);

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(x => 
                x.Email.Contains(searchTerm) ||
                x.FirstName.Contains(searchTerm) ||
                x.LastName.Contains(searchTerm));
        }

        var totalCount = await query.CountAsync(cancellationToken);
        
        var users = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (users, totalCount);
    }
}
