using Microsoft.EntityFrameworkCore;
using WebSearchIndexing.Modules.Identity.Domain.Entities;
using WebSearchIndexing.Modules.Identity.Domain.Repositories;
using WebSearchIndexing.Modules.Identity.Infrastructure.Persistence;

namespace WebSearchIndexing.Modules.Identity.Infrastructure.Persistence.Repositories;

public sealed class RoleRepository : IRoleRepository
{
    private readonly IdentityDbContext _context;

    public RoleRepository(IdentityDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<Role?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Roles
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }

    public async Task<Role?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        
        return await _context.Roles
            .FirstOrDefaultAsync(r => r.Name == name, cancellationToken);
    }

    public async Task<IReadOnlyCollection<Role>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var roles = await _context.Roles
            .Where(r => r.IsActive)
            .ToListAsync(cancellationToken);
        
        return roles.AsReadOnly();
    }

    public async Task<IReadOnlyCollection<Role>> GetByTypeAsync(RoleType type, CancellationToken cancellationToken = default)
    {
        var roles = await _context.Roles
            .Where(r => r.Type == type && r.IsActive)
            .ToListAsync(cancellationToken);
        
        return roles.AsReadOnly();
    }

    public async Task<IReadOnlyCollection<Role>> GetGlobalRolesAsync(CancellationToken cancellationToken = default)
    {
        return await GetByTypeAsync(RoleType.Global, cancellationToken);
    }

    public async Task<IReadOnlyCollection<Role>> GetTenantRolesAsync(CancellationToken cancellationToken = default)
    {
        return await GetByTypeAsync(RoleType.Tenant, cancellationToken);
    }

    public async Task<IReadOnlyCollection<Role>> GetDomainRolesAsync(CancellationToken cancellationToken = default)
    {
        return await GetByTypeAsync(RoleType.Domain, cancellationToken);
    }

    public async Task<Role> AddAsync(Role role, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(role);
        
        await _context.Roles.AddAsync(role, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return role;
    }

    public async Task<Role> UpdateAsync(Role role, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(role);
        
        _context.Roles.Update(role);
        await _context.SaveChangesAsync(cancellationToken);
        return role;
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var role = await GetByIdAsync(id, cancellationToken);
        if (role != null)
        {
            _context.Roles.Remove(role);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        
        return await _context.Roles
            .AnyAsync(r => r.Name == name, cancellationToken);
    }
}