using System.Collections.Concurrent;
using WebSearchIndexing.Modules.Identity.Application.Security;

namespace WebSearchIndexing.Modules.Identity.Infrastructure.Security;

/// <summary>
/// Configuration options for account lockout
/// </summary>
public sealed class AccountLockoutOptions
{
    /// <summary>
    /// Maximum number of failed login attempts before lockout
    /// </summary>
    public int MaxFailedAttempts { get; set; } = 5;

    /// <summary>
    /// Lockout duration in minutes
    /// </summary>
    public int LockoutDurationMinutes { get; set; } = 30;

    /// <summary>
    /// Time window in minutes for tracking failed attempts
    /// </summary>
    public int FailedAttemptsWindowMinutes { get; set; } = 15;
}

/// <summary>
/// Service for managing account lockouts
/// </summary>
public interface IAccountLockoutService
{
    /// <summary>
    /// Records a failed login attempt
    /// </summary>
    Task<bool> RecordFailedAttemptAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if an account is locked out
    /// </summary>
    Task<bool> IsLockedOutAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the remaining lockout time
    /// </summary>
    Task<TimeSpan?> GetRemainingLockoutTimeAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Resets failed attempts for an account
    /// </summary>
    Task ResetFailedAttemptsAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Unlocks an account manually
    /// </summary>
    Task UnlockAccountAsync(string email, CancellationToken cancellationToken = default);
}

/// <summary>
/// In-memory implementation of account lockout service
/// </summary>
public sealed class InMemoryAccountLockoutService : IAccountLockoutService
{
    private readonly ConcurrentDictionary<string, LockoutEntry> _entries = new();
    private readonly AccountLockoutOptions _options;
    private readonly ISecurityLoggingService _securityLogging;

    public InMemoryAccountLockoutService(
        AccountLockoutOptions options,
        ISecurityLoggingService securityLogging)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _securityLogging = securityLogging ?? throw new ArgumentNullException(nameof(securityLogging));
    }

    public Task<bool> RecordFailedAttemptAsync(string email, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(email);

        var key = email.ToLowerInvariant();
        var entry = _entries.GetOrAdd(key, _ => new LockoutEntry
        {
            FailedAttempts = 0,
            WindowStart = DateTime.UtcNow,
            LockedUntil = null
        });

        lock (entry)
        {
            // Check if already locked
            if (entry.LockedUntil.HasValue && DateTime.UtcNow < entry.LockedUntil.Value)
            {
                return Task.FromResult(true); // Already locked
            }

            // Reset if window expired
            if (DateTime.UtcNow - entry.WindowStart > TimeSpan.FromMinutes(_options.FailedAttemptsWindowMinutes))
            {
                entry.FailedAttempts = 0;
                entry.WindowStart = DateTime.UtcNow;
            }

            // Increment failed attempts
            entry.FailedAttempts++;

            // Check if should lock
            if (entry.FailedAttempts >= _options.MaxFailedAttempts)
            {
                entry.LockedUntil = DateTime.UtcNow.AddMinutes(_options.LockoutDurationMinutes);
                
                _securityLogging.LogSecurityEvent(new SecurityEvent
                {
                    EventType = SecurityEventType.AccountLockout,
                    Email = email,
                    AdditionalInfo = $"Account locked after {entry.FailedAttempts} failed attempts. Locked until {entry.LockedUntil:u}"
                });

                return Task.FromResult(true); // Locked
            }

            return Task.FromResult(false); // Not locked yet
        }
    }

    public Task<bool> IsLockedOutAsync(string email, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(email);

        var key = email.ToLowerInvariant();
        if (!_entries.TryGetValue(key, out var entry))
        {
            return Task.FromResult(false);
        }

        lock (entry)
        {
            if (!entry.LockedUntil.HasValue)
            {
                return Task.FromResult(false);
            }

            if (DateTime.UtcNow >= entry.LockedUntil.Value)
            {
                // Lockout expired
                entry.LockedUntil = null;
                entry.FailedAttempts = 0;
                entry.WindowStart = DateTime.UtcNow;
                return Task.FromResult(false);
            }

            return Task.FromResult(true);
        }
    }

    public Task<TimeSpan?> GetRemainingLockoutTimeAsync(string email, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(email);

        var key = email.ToLowerInvariant();
        if (!_entries.TryGetValue(key, out var entry))
        {
            return Task.FromResult<TimeSpan?>(null);
        }

        lock (entry)
        {
            if (!entry.LockedUntil.HasValue || DateTime.UtcNow >= entry.LockedUntil.Value)
            {
                return Task.FromResult<TimeSpan?>(null);
            }

            return Task.FromResult<TimeSpan?>(entry.LockedUntil.Value - DateTime.UtcNow);
        }
    }

    public Task ResetFailedAttemptsAsync(string email, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(email);

        var key = email.ToLowerInvariant();
        if (_entries.TryGetValue(key, out var entry))
        {
            lock (entry)
            {
                entry.FailedAttempts = 0;
                entry.WindowStart = DateTime.UtcNow;
                entry.LockedUntil = null;
            }
        }

        return Task.CompletedTask;
    }

    public Task UnlockAccountAsync(string email, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(email);

        var key = email.ToLowerInvariant();
        if (_entries.TryGetValue(key, out var entry))
        {
            lock (entry)
            {
                entry.LockedUntil = null;
                entry.FailedAttempts = 0;
                entry.WindowStart = DateTime.UtcNow;

                _securityLogging.LogSecurityEvent(new SecurityEvent
                {
                    EventType = SecurityEventType.AccountUnlock,
                    Email = email,
                    AdditionalInfo = "Account manually unlocked"
                });
            }
        }

        return Task.CompletedTask;
    }

    private sealed class LockoutEntry
    {
        public int FailedAttempts { get; set; }
        public DateTime WindowStart { get; set; }
        public DateTime? LockedUntil { get; set; }
    }
}
