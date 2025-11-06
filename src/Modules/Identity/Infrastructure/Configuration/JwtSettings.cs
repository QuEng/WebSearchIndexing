namespace WebSearchIndexing.Modules.Identity.Infrastructure.Configuration;

public sealed class JwtSettings
{
    public const string SectionName = "Jwt";

    public string Key { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public int AccessTokenExpiryMinutes { get; set; } = 30; // Shorter for security
    public int RefreshTokenExpiryDays { get; set; } = 7;
    public string RefreshTokenCookieName { get; set; } = "refresh_token";
}
