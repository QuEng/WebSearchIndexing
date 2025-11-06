using Microsoft.EntityFrameworkCore;
using WebSearchIndexing.Modules.Identity.Domain.Entities;
using WebSearchIndexing.Modules.Identity.Domain.Repositories;
using WebSearchIndexing.Modules.Identity.Infrastructure.Persistence;

namespace WebSearchIndexing.Modules.Identity.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository implementation for email verification tokens
/// </summary>
public sealed class EmailVerificationRepository : IEmailVerificationRepository
{
    private readonly IdentityDbContext _context;

    public EmailVerificationRepository(IdentityDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<EmailVerificationToken> CreateTokenAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _context.Users.FindAsync([userId], cancellationToken)
            ?? throw new InvalidOperationException($"User with ID {userId} not found");

        var token = new EmailVerificationToken(userId, user.Email);
        
        await _context.EmailVerificationTokens.AddAsync(token, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return token;
    }

    public async Task<EmailVerificationToken?> GetByTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(token);

        return await _context.EmailVerificationTokens
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.Token == token, cancellationToken);
    }

    public async Task<IReadOnlyCollection<EmailVerificationToken>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var tokens = await _context.EmailVerificationTokens
            .Where(t => t.UserId == userId)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync(cancellationToken);

        return tokens.AsReadOnly();
    }

    public async Task MarkAsUsedAsync(Guid tokenId, CancellationToken cancellationToken = default)
    {
        var token = await _context.EmailVerificationTokens.FindAsync([tokenId], cancellationToken)
            ?? throw new InvalidOperationException($"Token with ID {tokenId} not found");

        token.MarkAsUsed();
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteExpiredTokensAsync(CancellationToken cancellationToken = default)
    {
        var expiredTokens = await _context.EmailVerificationTokens
            .Where(t => t.ExpiresAt < DateTime.UtcNow)
            .ToListAsync(cancellationToken);

        _context.EmailVerificationTokens.RemoveRange(expiredTokens);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
