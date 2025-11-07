using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using WebSearchIndexing.Modules.Identity.Application.Abstractions;
using WebSearchIndexing.Modules.Identity.Domain.Entities;
using WebSearchIndexing.Modules.Identity.Infrastructure.Configuration;

namespace WebSearchIndexing.Modules.Identity.Infrastructure.Services;

internal sealed class TokenService : ITokenService
{
    private readonly JwtSettings _jwtSettings;
    private readonly SigningCredentials _signingCredentials;
    private readonly IRefreshTokenRepository _refreshTokenRepository;

    public TokenService(
        IOptions<JwtSettings> jwtSettings,
        IRefreshTokenRepository refreshTokenRepository)
    {
        ArgumentNullException.ThrowIfNull(jwtSettings);
        ArgumentNullException.ThrowIfNull(refreshTokenRepository);
        
        _jwtSettings = jwtSettings.Value;
        _refreshTokenRepository = refreshTokenRepository;
        
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key));
        _signingCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
    }

    public Task<string> GenerateAccessTokenAsync(User user, Guid? tenantId = null, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(user);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(JwtRegisteredClaimNames.Name, $"{user.FirstName} {user.LastName}".Trim()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Iat, 
                new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds().ToString(), 
                ClaimValueTypes.Integer64),
            new("first_name", user.FirstName),
            new("last_name", user.LastName),
            new("user_id", user.Id.ToString())
        };

        // Add tenant claim if provided
        if (tenantId.HasValue)
        {
            claims.Add(new("tenant_id", tenantId.Value.ToString()));
        }

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpiryMinutes),
            Issuer = _jwtSettings.Issuer,
            Audience = _jwtSettings.Audience,
            SigningCredentials = _signingCredentials
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);

        return Task.FromResult(tokenHandler.WriteToken(token));
    }

    public async Task<AuthResult> AuthenticateAsync(
        User user, 
        AuthenticationContext context, 
        Guid? tenantId = null, 
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(user);
        ArgumentNullException.ThrowIfNull(context);

        try
        {
            // Revoke existing refresh tokens for security
            await RevokeAllUserTokensAsync(user.Id, "New login", cancellationToken);

            // Generate new tokens
            var accessToken = await GenerateAccessTokenAsync(user, tenantId, cancellationToken);
            var refreshTokenValue = GenerateSecureRefreshToken();
            
            // Create refresh token entity
            var refreshToken = new RefreshToken(
                refreshTokenValue,
                user.Id,
                DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpiryDays),
                context.IpAddress,
                tenantId);

            // Store refresh token in database
            await _refreshTokenRepository.AddAsync(refreshToken, cancellationToken);

            // Set HTTP-Only cookie
            var cookieOptions = new Application.Abstractions.CookieOptions(
                HttpOnly: true,
                Secure: true,
                SameSite: "Strict",
                Expires: refreshToken.ExpiresAt);

            context.SetCookie(_jwtSettings.RefreshTokenCookieName, refreshTokenValue, cookieOptions);

            var expiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpiryMinutes);
            
            return new AuthResult(
                AccessToken: accessToken,
                ExpiresIn: _jwtSettings.AccessTokenExpiryMinutes * 60, // in seconds
                ExpiresAt: expiresAt,
                Success: true);
        }
        catch (Exception ex)
        {
            return new AuthResult(
                AccessToken: string.Empty,
                ExpiresIn: 0,
                ExpiresAt: DateTime.UtcNow,
                Success: false,
                Error: $"Authentication failed: {ex.Message}");
        }
    }

    public async Task<AuthResult?> RefreshTokenAsync(
        string refreshTokenValue, 
        AuthenticationContext context, 
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(refreshTokenValue))
            return null;

        try
        {
            var refreshToken = await _refreshTokenRepository.GetByTokenAsync(refreshTokenValue, cancellationToken);
            
            if (refreshToken == null || !refreshToken.IsValid)
                return null;

            // Generate new access token
            var accessToken = await GenerateAccessTokenAsync(refreshToken.User, refreshToken.TenantId, cancellationToken);
            
            // Generate new refresh token for rotation
            var newRefreshTokenValue = GenerateSecureRefreshToken();
            
            // Revoke old refresh token
            refreshToken.Revoke("Token rotated");
            await _refreshTokenRepository.UpdateAsync(refreshToken, cancellationToken);
            
            // Create new refresh token
            var newRefreshToken = new RefreshToken(
                newRefreshTokenValue,
                refreshToken.UserId,
                DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpiryDays),
                context.IpAddress,
                refreshToken.TenantId);

            await _refreshTokenRepository.AddAsync(newRefreshToken, cancellationToken);

            // Update HTTP-Only cookie
            var cookieOptions = new Application.Abstractions.CookieOptions(
                HttpOnly: true,
                Secure: true,
                SameSite: "Strict",
                Expires: newRefreshToken.ExpiresAt);

            context.SetCookie(_jwtSettings.RefreshTokenCookieName, newRefreshTokenValue, cookieOptions);

            var expiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpiryMinutes);
            
            return new AuthResult(
                AccessToken: accessToken,
                ExpiresIn: _jwtSettings.AccessTokenExpiryMinutes * 60,
                ExpiresAt: expiresAt,
                Success: true);
        }
        catch (Exception ex)
        {
            return new AuthResult(
                AccessToken: string.Empty,
                ExpiresIn: 0,
                ExpiresAt: DateTime.UtcNow,
                Success: false,
                Error: $"Token refresh failed: {ex.Message}");
        }
    }

    public Task<Application.Abstractions.TokenValidationResult> ValidateTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return Task.FromResult(new Application.Abstractions.TokenValidationResult(false, Error: "Token is null or empty"));
        }

        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_jwtSettings.Key);

            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = _jwtSettings.Issuer,
                ValidateAudience = true,
                ValidAudience = _jwtSettings.Audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };

            var principal = tokenHandler.ValidateToken(token, validationParameters, out var validatedToken);

            if (validatedToken is not JwtSecurityToken jwtToken ||
                !jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                return Task.FromResult(new Application.Abstractions.TokenValidationResult(false, Error: "Invalid token algorithm"));
            }

            var userIdClaim = principal.FindFirst("user_id") ?? principal.FindFirst(JwtRegisteredClaimNames.Sub);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
            {
                return Task.FromResult(new Application.Abstractions.TokenValidationResult(false, Error: "Invalid user ID in token"));
            }

            // Check for tenant ID if present
            var tenantIdClaim = principal.FindFirst("tenant_id");
            Guid? tenantId = null;
            if (tenantIdClaim != null && Guid.TryParse(tenantIdClaim.Value, out var parsedTenantId))
            {
                tenantId = parsedTenantId;
            }

            return Task.FromResult(new Application.Abstractions.TokenValidationResult(true, userId, tenantId));
        }
        catch (SecurityTokenExpiredException)
        {
            return Task.FromResult(new Application.Abstractions.TokenValidationResult(false, Error: "Token has expired"));
        }
        catch (SecurityTokenException ex)
        {
            return Task.FromResult(new Application.Abstractions.TokenValidationResult(false, Error: $"Token validation failed: {ex.Message}"));
        }
        catch (Exception ex)
        {
            return Task.FromResult(new Application.Abstractions.TokenValidationResult(false, Error: $"Unexpected error: {ex.Message}"));
        }
    }

    public async Task RevokeRefreshTokenAsync(string refreshTokenValue, string reason, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(refreshTokenValue))
            return;

        var refreshToken = await _refreshTokenRepository.GetByTokenAsync(refreshTokenValue, cancellationToken);
        if (refreshToken != null && !refreshToken.IsRevoked)
        {
            refreshToken.Revoke(reason);
            await _refreshTokenRepository.UpdateAsync(refreshToken, cancellationToken);
        }
    }

    public async Task RevokeAllUserTokensAsync(Guid userId, string reason, CancellationToken cancellationToken = default)
    {
        await _refreshTokenRepository.RevokeAllUserTokensAsync(userId, reason, cancellationToken);
    }

    private static string GenerateSecureRefreshToken()
    {
        return Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
    }
}
