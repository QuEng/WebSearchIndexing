using WebSearchIndexing.Modules.Identity.Ui.Services;

namespace WebSearchIndexing.Modules.Identity.Ui.State;

public class AuthenticationState
{
    private readonly ISecureTokenStorage _tokenStorage;
    
    public AuthenticationState(ISecureTokenStorage tokenStorage)
    {
        _tokenStorage = tokenStorage;
    }
    
    public bool IsAuthenticated => _tokenStorage.IsAccessTokenValid();
    public DateTime? AccessTokenExpiresAt => _tokenStorage.GetAccessTokenExpiry();
    public TimeSpan? TimeUntilExpiry => AccessTokenExpiresAt?.Subtract(DateTime.UtcNow);
    public bool IsExpiringSoon => TimeUntilExpiry?.TotalMinutes <= 5;
    public bool IsExpired => AccessTokenExpiresAt <= DateTime.UtcNow;
    public string ExpiryStatus => IsExpired ? "Expired" : IsExpiringSoon ? "Expiring Soon" : "Valid";
    
    // Auto-refresh countdown for UI
    public int MinutesUntilExpiry => TimeUntilExpiry?.Minutes ?? 0;
    public int SecondsUntilExpiry => TimeUntilExpiry?.Seconds ?? 0;
    
    public string FormattedTimeUntilExpiry
    {
        get
        {
            if (!TimeUntilExpiry.HasValue || TimeUntilExpiry.Value.TotalSeconds <= 0)
                return "Expired";
                
            var timeSpan = TimeUntilExpiry.Value;
            
            if (timeSpan.TotalMinutes < 1)
                return "< 1 min";
            if (timeSpan.TotalHours < 1)
                return $"{(int)timeSpan.TotalMinutes} min";
            if (timeSpan.TotalDays < 1)
                return $"{(int)timeSpan.TotalHours}h {timeSpan.Minutes}m";
            
            return $"{(int)timeSpan.TotalDays}d {timeSpan.Hours}h";
        }
    }
    
    public event Action? StateChanged;
    
    public void NotifyStateChanged()
    {
        StateChanged?.Invoke();
    }
}
