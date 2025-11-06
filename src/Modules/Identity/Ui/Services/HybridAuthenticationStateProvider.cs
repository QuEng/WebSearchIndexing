using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Components.Authorization;

namespace WebSearchIndexing.Modules.Identity.Ui.Services;

public class HybridAuthenticationStateProvider : AuthenticationStateProvider
{
    private readonly HttpClient _httpClient;
    private readonly ISecureTokenStorage _tokenStorage;
    private ClaimsPrincipal _anonymous = new(new ClaimsIdentity());

    public HybridAuthenticationStateProvider(HttpClient httpClient, ISecureTokenStorage tokenStorage)
    {
        _httpClient = httpClient;
        _tokenStorage = tokenStorage;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        try
        {
            // Initialize token storage if needed
            await _tokenStorage.InitializeAsync();
            
            // First check if we have a valid access token
            var accessToken = await _tokenStorage.GetAccessTokenAsync();
            
            if (!string.IsNullOrEmpty(accessToken))
            {
                // Parse claims from token
                var claims = ParseClaimsFromJwt(accessToken);
                var identity = new ClaimsIdentity(claims, "jwt");
                var user = new ClaimsPrincipal(identity);
                return new AuthenticationState(user);
            }

            // If no valid access token, try to refresh using cookies
            var refreshResult = await TryRefreshTokenAsync();
            if (refreshResult.IsSuccess && !string.IsNullOrEmpty(refreshResult.AccessToken))
            {
                var claims = ParseClaimsFromJwt(refreshResult.AccessToken);
                var identity = new ClaimsIdentity(claims, "jwt");
                var user = new ClaimsPrincipal(identity);
                return new AuthenticationState(user);
            }

            // No valid authentication
            return new AuthenticationState(_anonymous);
        }
        catch
        {
            return new AuthenticationState(_anonymous);
        }
    }

    private async Task<AuthResult> TryRefreshTokenAsync()
    {
        try
        {
            var response = await _httpClient.PostAsync("/api/v1/identity/auth/refresh", null);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var refreshResponse = JsonSerializer.Deserialize<RefreshResponse>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (refreshResponse?.AccessToken != null && refreshResponse.ExpiresAt.HasValue)
                {
                    _tokenStorage.SetAccessToken(refreshResponse.AccessToken, refreshResponse.ExpiresAt.Value);
                    return AuthResult.Success(refreshResponse.AccessToken, refreshResponse.ExpiresAt.Value);
                }
            }

            return AuthResult.Failure("Refresh failed");
        }
        catch (Exception ex)
        {
            return AuthResult.Failure(ex.Message);
        }
    }

    public async Task NotifyAuthenticationStateChangedAsync()
    {
        var authState = await GetAuthenticationStateAsync();
        NotifyAuthenticationStateChanged(Task.FromResult(authState));
    }

    private static IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
    {
        var claims = new List<Claim>();
        
        try
        {
            var payload = jwt.Split('.')[1];
            
            // Add padding if needed
            switch (payload.Length % 4)
            {
                case 2: payload += "=="; break;
                case 3: payload += "="; break;
            }
            
            var jsonBytes = Convert.FromBase64String(payload);
            var keyValuePairs = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonBytes);

            if (keyValuePairs != null)
            {
                foreach (var kvp in keyValuePairs)
                {
                    var claimType = kvp.Key switch
                    {
                        "sub" => ClaimTypes.NameIdentifier,
                        "email" => ClaimTypes.Email,
                        "given_name" => ClaimTypes.GivenName,
                        "family_name" => ClaimTypes.Surname,
                        "role" => ClaimTypes.Role,
                        _ => kvp.Key
                    };

                    var claimValue = kvp.Value?.ToString() ?? string.Empty;
                    claims.Add(new Claim(claimType, claimValue));
                }
            }
        }
        catch
        {
            // If JWT parsing fails, return empty claims
        }

        return claims;
    }

    private class RefreshResponse
    {
        public string? AccessToken { get; set; }
        public DateTime? ExpiresAt { get; set; }
    }
}
