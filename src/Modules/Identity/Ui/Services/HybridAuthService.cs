using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Components.Authorization;

namespace WebSearchIndexing.Modules.Identity.Ui.Services;

public class HybridAuthService
{
    private readonly HttpClient _httpClient;
    private readonly ISecureTokenStorage _tokenStorage;
    private readonly AuthenticationStateProvider _authStateProvider;

    public HybridAuthService(
        IHttpClientFactory httpClientFactory, 
        ISecureTokenStorage tokenStorage,
        AuthenticationStateProvider authStateProvider)
    {
        // Use AuthApi client without interceptor to avoid circular dependency
        _httpClient = httpClientFactory.CreateClient("AuthApi");
        _tokenStorage = tokenStorage;
        _authStateProvider = authStateProvider;
    }

    public async Task<AuthResult> LoginAsync(string email, string password, bool rememberMe = false)
    {
        try
        {
            // Set remember me preference before making the request
            _tokenStorage.SetRememberMe(rememberMe);
            
            var loginRequest = new
            {
                Email = email,
                Password = password,
                RememberMe = rememberMe
            };

            var response = await _httpClient.PostAsJsonAsync("/api/v1/identity/auth/login", loginRequest);
            
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var loginResponse = JsonSerializer.Deserialize<LoginResponse>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (loginResponse?.AccessToken != null && loginResponse.ExpiresAt.HasValue)
                {
                    // Store access token in memory (and localStorage if rememberMe)
                    _tokenStorage.SetAccessToken(loginResponse.AccessToken, loginResponse.ExpiresAt.Value);
                    
                    // Refresh token is automatically set as HTTP-Only cookie by server
                    
                    // Notify authentication state changed
                    if (_authStateProvider is HybridAuthenticationStateProvider provider)
                    {
                        await provider.NotifyAuthenticationStateChangedAsync();
                    }

                    return AuthResult.Success(loginResponse.AccessToken, loginResponse.ExpiresAt.Value);
                }
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            return AuthResult.Failure($"Login failed: {response.StatusCode}");
        }
        catch (Exception ex)
        {
            return AuthResult.Failure($"Login error: {ex.Message}");
        }
    }

    public async Task<AuthResult> RegisterAsync(string email, string password, string firstName, string lastName)
    {
        try
        {
            var registerRequest = new
            {
                Email = email,
                Password = password,
                FirstName = firstName,
                LastName = lastName
            };

            var response = await _httpClient.PostAsJsonAsync("/api/v1/identity/auth/register", registerRequest);

            if (response.IsSuccessStatusCode)
            {
                return AuthResult.Success(string.Empty, DateTime.UtcNow);
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            // Try to parse error response for better error messages
            try
            {
                var errorResponse = JsonSerializer.Deserialize<ErrorResponse>(errorContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                return AuthResult.Failure(errorResponse?.Message ?? $"Registration failed: {response.StatusCode}");
            }
            catch
            {
                return AuthResult.Failure($"Registration failed: {response.StatusCode} - {errorContent}");
            }
        }
        catch (Exception ex)
        {
            return AuthResult.Failure($"Registration error: {ex.Message}");
        }
    }

    public async Task<AuthResult> RefreshTokenAsync()
    {
        try
        {
            // Refresh tokens are sent automatically via HTTP-Only cookies
            var response = await _httpClient.PostAsync("/api/v1/identity/auth/refresh", null);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var refreshResponse = JsonSerializer.Deserialize<LoginResponse>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (refreshResponse?.AccessToken != null && refreshResponse.ExpiresAt.HasValue)
                {
                    // Update access token in memory
                    _tokenStorage.SetAccessToken(refreshResponse.AccessToken, refreshResponse.ExpiresAt.Value);
                    
                    return AuthResult.Success(refreshResponse.AccessToken, refreshResponse.ExpiresAt.Value);
                }
            }

            return AuthResult.Failure("Token refresh failed");
        }
        catch (Exception ex)
        {
            return AuthResult.Failure($"Refresh error: {ex.Message}");
        }
    }

    public async Task<bool> LogoutAsync()
    {
        try
        {
            // Clear memory storage first
            _tokenStorage.ClearAll();

            // Call server logout to clear HTTP-Only cookies
            var response = await _httpClient.PostAsync("/api/v1/identity/auth/logout", null);

            // Notify authentication state changed regardless of server response
            if (_authStateProvider is HybridAuthenticationStateProvider provider)
            {
                await provider.NotifyAuthenticationStateChangedAsync();
            }

            return response.IsSuccessStatusCode;
        }
        catch
        {
            // Even if server call fails, we've cleared local storage
            return true;
        }
    }

    public bool IsAuthenticated => _tokenStorage.IsAccessTokenValid();

    private class LoginResponse
    {
        public string? AccessToken { get; set; }
        public DateTime? ExpiresAt { get; set; }
    }

    private class ErrorResponse
    {
        public string? Message { get; set; }
    }
}
