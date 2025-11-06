using WebSearchIndexing.Modules.Identity.Domain.Entities;

namespace WebSearchIndexing.Modules.Identity.Application.Abstractions;

public interface ITokenService
{
    Task<string> GenerateAccessTokenAsync(User user, Guid? tenantId = null, CancellationToken cancellationToken = default);
    Task<AuthResult> AuthenticateAsync(User user, AuthenticationContext context, Guid? tenantId = null, CancellationToken cancellationToken = default);
    Task<AuthResult?> RefreshTokenAsync(string refreshToken, AuthenticationContext context, CancellationToken cancellationToken = default);
    Task<TokenValidationResult> ValidateTokenAsync(string token, CancellationToken cancellationToken = default);
    Task RevokeRefreshTokenAsync(string refreshToken, string reason, CancellationToken cancellationToken = default);
    Task RevokeAllUserTokensAsync(Guid userId, string reason, CancellationToken cancellationToken = default);
}

public record TokenValidationResult(
    bool IsValid,
    Guid? UserId = null,
    Guid? TenantId = null,
    string? Error = null);

public record AuthResult(
    string AccessToken,
    int ExpiresIn,
    DateTime ExpiresAt,
    bool Success = true,
    string? Error = null);

public record AuthenticationContext(
    string IpAddress,
    string UserAgent,
    Action<string, string, CookieOptions> SetCookie,
    Action<string> DeleteCookie);

public record CookieOptions(
    bool HttpOnly = true,
    bool Secure = true,
    string SameSite = "Strict",
    DateTime? Expires = null);
