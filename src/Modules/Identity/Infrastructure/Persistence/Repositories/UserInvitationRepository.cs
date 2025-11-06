using Microsoft.EntityFrameworkCore;
using WebSearchIndexing.Modules.Identity.Domain.Repositories;
using WebSearchIndexing.Modules.Identity.Domain.Entities;
using WebSearchIndexing.Modules.Identity.Infrastructure.Persistence;

namespace WebSearchIndexing.Modules.Identity.Infrastructure.Persistence.Repositories;

public class UserInvitationRepository : IUserInvitationRepository
{
    private readonly IdentityDbContext _context;

    public UserInvitationRepository(IdentityDbContext context)
    {
        _context = context;
    }

    public async Task<UserInvitation?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.UserInvitations
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<UserInvitation?> GetByTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        return await _context.UserInvitations
            .FirstOrDefaultAsync(x => x.InvitationToken == token, cancellationToken);
    }

    public async Task<UserInvitation?> GetPendingByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _context.UserInvitations
            .FirstOrDefaultAsync(x => x.Email == email && !x.IsUsed && !x.IsRevoked && x.ExpiresAt > DateTime.UtcNow, cancellationToken);
    }

    public async Task<IReadOnlyCollection<UserInvitation>> GetByTenantAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        var invitations = await _context.UserInvitations
            .Where(x => x.TenantId == tenantId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);
        
        return invitations.AsReadOnly();
    }

    public async Task<IReadOnlyCollection<UserInvitation>> GetByDomainAsync(Guid domainId, CancellationToken cancellationToken = default)
    {
        var invitations = await _context.UserInvitations
            .Where(x => x.DomainId == domainId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);
        
        return invitations.AsReadOnly();
    }

    public async Task<IReadOnlyCollection<UserInvitation>> GetPendingInvitationsAsync(CancellationToken cancellationToken = default)
    {
        var invitations = await _context.UserInvitations
            .Where(x => !x.IsUsed && !x.IsRevoked && x.ExpiresAt > DateTime.UtcNow)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);
        
        return invitations.AsReadOnly();
    }

    public async Task<UserInvitation> AddAsync(UserInvitation invitation, CancellationToken cancellationToken = default)
    {
        await _context.UserInvitations.AddAsync(invitation, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return invitation;
    }

    public async Task<UserInvitation> UpdateAsync(UserInvitation invitation, CancellationToken cancellationToken = default)
    {
        _context.UserInvitations.Update(invitation);
        await _context.SaveChangesAsync(cancellationToken);
        return invitation;
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var invitation = await _context.UserInvitations.FindAsync(new object[] { id }, cancellationToken);
        if (invitation != null)
        {
            _context.UserInvitations.Remove(invitation);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
