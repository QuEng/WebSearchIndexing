namespace WebSearchIndexing.Modules.Identity.Ui.Services;

public class AuthResult
{
    public bool IsSuccess { get; set; }
    public string? ErrorMessage { get; set; }
    public string? AccessToken { get; set; }
    public DateTime? ExpiresAt { get; set; }

    public static AuthResult Success(string accessToken, DateTime expiresAt) => 
        new() { IsSuccess = true, AccessToken = accessToken, ExpiresAt = expiresAt };

    public static AuthResult Failure(string errorMessage) => 
        new() { IsSuccess = false, ErrorMessage = errorMessage };
}
