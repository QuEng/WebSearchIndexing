namespace WebSearchIndexing.Modules.Identity.Ui.Services;

public interface ISecureTokenStorage
{
    void SetAccessToken(string token, DateTime expiresAt);
    void SetRememberMe(bool remember);
    string? GetAccessToken();
    Task<string?> GetAccessTokenAsync();
    DateTime? GetAccessTokenExpiry();
    bool IsAccessTokenValid();
    void ClearAccessToken();
    void ClearAll();
    Task InitializeAsync();
}
