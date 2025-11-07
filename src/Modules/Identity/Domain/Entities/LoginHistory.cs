using WebSearchIndexing.Modules.Identity.Domain.Common;

namespace WebSearchIndexing.Modules.Identity.Domain.Entities;

public class LoginHistory : Entity<Guid>
{
    public Guid UserId { get; private set; }
    public string IpAddress { get; private set; } = string.Empty;
    public string UserAgent { get; private set; } = string.Empty;
    public string? Location { get; private set; }
    public DateTime LoginAt { get; private set; }
    public bool IsSuccessful { get; private set; }
    public string? FailureReason { get; private set; }
    public string? DeviceInfo { get; private set; }
    public Guid? TenantId { get; private set; }

    // Navigation properties
    public User User { get; private set; } = default!;

    // Private constructor for EF Core
    private LoginHistory() { }

    public LoginHistory(
        Guid userId,
        string ipAddress,
        string userAgent,
        bool isSuccessful,
        string? location = null,
        string? failureReason = null,
        string? deviceInfo = null,
        Guid? tenantId = null)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        IpAddress = ipAddress ?? throw new ArgumentNullException(nameof(ipAddress));
        UserAgent = userAgent ?? throw new ArgumentNullException(nameof(userAgent));
        Location = location;
        LoginAt = DateTime.UtcNow;
        IsSuccessful = isSuccessful;
        FailureReason = failureReason;
        DeviceInfo = deviceInfo;
        TenantId = tenantId;
    }

    public static LoginHistory CreateSuccessful(
        Guid userId,
        string ipAddress,
        string userAgent,
        string? location = null,
        string? deviceInfo = null,
        Guid? tenantId = null)
    {
        return new LoginHistory(
            userId, 
            ipAddress, 
            userAgent, 
            true, 
            location, 
            null, 
            deviceInfo, 
            tenantId);
    }

    public static LoginHistory CreateFailed(
        Guid userId,
        string ipAddress,
        string userAgent,
        string failureReason,
        string? location = null,
        string? deviceInfo = null,
        Guid? tenantId = null)
    {
        return new LoginHistory(
            userId, 
            ipAddress, 
            userAgent, 
            false, 
            location, 
            failureReason, 
            deviceInfo, 
            tenantId);
    }
}
