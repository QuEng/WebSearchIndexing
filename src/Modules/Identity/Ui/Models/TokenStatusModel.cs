namespace WebSearchIndexing.Modules.Identity.Ui.Models;

public class TokenStatusModel
{
    public bool IsAuthenticated { get; set; }
    public DateTime? AccessTokenExpiresAt { get; set; }
    public bool HasRefreshToken { get; set; }
    public TimeSpan? TimeUntilExpiry => AccessTokenExpiresAt?.Subtract(DateTime.UtcNow);
    public bool IsExpiringSoon => TimeUntilExpiry?.TotalMinutes <= 5;
    public bool IsExpired => AccessTokenExpiresAt <= DateTime.UtcNow;
    public string ExpiryStatus => IsExpired ? "Expired" : IsExpiringSoon ? "Expiring Soon" : "Valid";
}
