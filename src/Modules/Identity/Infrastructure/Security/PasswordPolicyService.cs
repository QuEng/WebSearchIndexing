using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Text.RegularExpressions;
using WebSearchIndexing.Modules.Identity.Application.Abstractions;
using WebSearchIndexing.Modules.Identity.Application.Security;
using WebSearchIndexing.Modules.Identity.Domain.Entities;
using WebSearchIndexing.Modules.Identity.Infrastructure.Persistence;

namespace WebSearchIndexing.Modules.Identity.Infrastructure.Security;

/// <summary>
/// Implementation of password policy service
/// </summary>
public sealed class PasswordPolicyService : IPasswordPolicyService
{
    private readonly PasswordPolicyOptions _options;
    private readonly IdentityDbContext _dbContext;
    private readonly IPasswordHasher _passwordHasher;

    public PasswordPolicyService(
        IOptions<PasswordPolicyOptions> options,
        IdentityDbContext dbContext,
        IPasswordHasher passwordHasher)
    {
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));
    }

    public PasswordValidationResult ValidatePassword(string password, string? email = null)
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            return PasswordValidationResult.Failure("Password is required");
        }

        var errors = new List<string>();

        // Length check
        if (password.Length < _options.MinimumLength)
        {
            errors.Add($"Password must be at least {_options.MinimumLength} characters long");
        }

        if (password.Length > _options.MaximumLength)
        {
            errors.Add($"Password must not exceed {_options.MaximumLength} characters");
        }

        // Uppercase check
        if (_options.RequireUppercase && !Regex.IsMatch(password, @"[A-Z]"))
        {
            errors.Add("Password must contain at least one uppercase letter");
        }

        // Lowercase check
        if (_options.RequireLowercase && !Regex.IsMatch(password, @"[a-z]"))
        {
            errors.Add("Password must contain at least one lowercase letter");
        }

        // Digit check
        if (_options.RequireDigit && !Regex.IsMatch(password, @"\d"))
        {
            errors.Add("Password must contain at least one digit");
        }

        // Special character check
        if (_options.RequireSpecialCharacter && !Regex.IsMatch(password, @"[!@#$%^&*()_+\-=\[\]{};':""\\|,.<>\/?]"))
        {
            errors.Add("Password must contain at least one special character");
        }

        // Check against common passwords
        if (_options.CommonPasswordBlacklist.Any(common => 
            password.Equals(common, StringComparison.OrdinalIgnoreCase)))
        {
            errors.Add("This password is too common. Please choose a more secure password");
        }

        // Check if password contains email
        if (!string.IsNullOrWhiteSpace(email))
        {
            var emailUsername = email.Split('@')[0];
            if (password.Contains(emailUsername, StringComparison.OrdinalIgnoreCase))
            {
                errors.Add("Password should not contain your email address");
            }
        }

        return errors.Any() 
            ? PasswordValidationResult.Failure(errors.ToArray()) 
            : PasswordValidationResult.Success();
    }

    public async Task<bool> CanChangePasswordAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        if (_options.MinimumPasswordAgeDays <= 0)
        {
            return true; // No minimum age restriction
        }

        var user = await _dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user?.LastPasswordChangeAt == null)
        {
            return true; // Never changed password, can change now
        }

        var minimumChangeDate = user.LastPasswordChangeAt.Value.AddDays(_options.MinimumPasswordAgeDays);
        return DateTime.UtcNow >= minimumChangeDate;
    }

    public async Task<bool> IsPasswordExpiredAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        if (_options.MaximumPasswordAgeDays <= 0)
        {
            return false; // No expiration
        }

        var user = await _dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user?.LastPasswordChangeAt == null)
        {
            // Consider password as created at user creation
            var passwordAge = (DateTime.UtcNow - user!.CreatedAt).TotalDays;
            return passwordAge > _options.MaximumPasswordAgeDays;
        }

        var daysSinceChange = (DateTime.UtcNow - user.LastPasswordChangeAt.Value).TotalDays;
        return daysSinceChange > _options.MaximumPasswordAgeDays;
    }

    public async Task RecordPasswordChangeAsync(Guid userId, string passwordHash, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(passwordHash);

        var passwordHistory = new PasswordHistory(userId, passwordHash);
        _dbContext.PasswordHistory.Add(passwordHistory);

        // Clean up old history beyond the limit
        if (_options.PasswordHistoryLimit > 0)
        {
            var excessHistory = await _dbContext.PasswordHistory
                .Where(ph => ph.UserId == userId)
                .OrderByDescending(ph => ph.CreatedAt)
                .Skip(_options.PasswordHistoryLimit)
                .ToListAsync(cancellationToken);

            _dbContext.PasswordHistory.RemoveRange(excessHistory);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> WasPasswordUsedRecentlyAsync(Guid userId, string passwordHash, CancellationToken cancellationToken = default)
    {
        if (_options.PasswordHistoryLimit <= 0)
        {
            return false; // No history tracking
        }

        var recentPasswords = await _dbContext.PasswordHistory
            .Where(ph => ph.UserId == userId)
            .OrderByDescending(ph => ph.CreatedAt)
            .Take(_options.PasswordHistoryLimit)
            .Select(ph => ph.PasswordHash)
            .ToListAsync(cancellationToken);

        // Check if the new password matches any recent password
        foreach (var oldHash in recentPasswords)
        {
            if (oldHash == passwordHash)
            {
                return true;
            }
        }

        return false;
    }
}
