using Microsoft.EntityFrameworkCore;
using WebSearchIndexing.Modules.Identity.Domain.Entities;
using WebSearchIndexing.Modules.Identity.Domain.Repositories;
using WebSearchIndexing.Modules.Identity.Infrastructure.Persistence;

namespace WebSearchIndexing.Modules.Identity.Infrastructure.Persistence.Repositories;

public class TenantRepository : ITenantRepository
{
    private readonly IdentityDbContext _context;

    public TenantRepository(IdentityDbContext context)
    {
        _context = context;
    }

    public async Task<Tenant?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Tenants
            .Include(x => x.UserTenants)
                .ThenInclude(x => x.User)
            .Include(x => x.UserTenants)
                .ThenInclude(x => x.Role)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<Tenant?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        return await _context.Tenants
            .Include(x => x.UserTenants)
                .ThenInclude(x => x.User)
            .Include(x => x.UserTenants)
                .ThenInclude(x => x.Role)
            .FirstOrDefaultAsync(x => x.Slug == slug, cancellationToken);
    }

    public async Task<IReadOnlyCollection<Tenant>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var tenants = await _context.Tenants
            .Include(x => x.UserTenants)
                .ThenInclude(x => x.User)
            .Where(x => x.IsActive)
            .ToListAsync(cancellationToken);
        
        return tenants.AsReadOnly();
    }

    public async Task<IEnumerable<Tenant>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.Tenants
            .Include(x => x.UserTenants)
                .ThenInclude(x => x.Role)
            .Where(x => x.UserTenants.Any(ut => ut.UserId == userId && ut.IsActive) && x.IsActive)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Tenant>> GetByOwnerAsync(Guid ownerId, CancellationToken cancellationToken = default)
    {
        // Assuming owner is a user with specific role like "TenantAdmin" or "Owner"
        return await _context.Tenants
            .Include(x => x.UserTenants)
            .Where(x => x.UserTenants.Any(ut => 
                ut.UserId == ownerId && 
                ut.IsActive && 
                (ut.Role == "TenantAdmin" || ut.Role == "Owner")) && x.IsActive)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Tenant>> GetByUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await GetByUserIdAsync(userId, cancellationToken);
    }

    public async Task<bool> ExistsBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        return await _context.Tenants
            .AnyAsync(x => x.Slug == slug, cancellationToken);
    }

    public async Task<Tenant> AddAsync(Tenant tenant, CancellationToken cancellationToken = default)
    {
        await _context.Tenants.AddAsync(tenant, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return tenant;
    }

    public async Task<Tenant> UpdateAsync(Tenant tenant, CancellationToken cancellationToken = default)
    {
        _context.Tenants.Update(tenant);
        await _context.SaveChangesAsync(cancellationToken);
        return tenant;
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var tenant = await GetByIdAsync(id, cancellationToken);
        if (tenant != null)
        {
            _context.Tenants.Remove(tenant);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        return await _context.Tenants
            .AnyAsync(x => x.Name == name, cancellationToken);
    }

    public async Task<(IEnumerable<Tenant> Tenants, int TotalCount)> GetPagedAsync(
        int page, 
        int pageSize, 
        string? searchTerm = null, 
        CancellationToken cancellationToken = default)
    {
        var query = _context.Tenants
            .Include(x => x.UserTenants)
                .ThenInclude(x => x.User)
            .Where(x => x.IsActive);

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(x => 
                x.Name.Contains(searchTerm) ||
                x.Slug.Contains(searchTerm));
        }

        var totalCount = await query.CountAsync(cancellationToken);
        
        var tenants = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (tenants, totalCount);
    }

    public void Update(Tenant tenant)
    {
        _context.Tenants.Update(tenant);
    }

    public void Delete(Tenant tenant)
    {
        _context.Tenants.Remove(tenant);
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }
}
