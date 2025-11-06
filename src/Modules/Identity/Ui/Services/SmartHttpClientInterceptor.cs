using System.Net;
using System.Net.Http.Headers;
using Microsoft.Extensions.DependencyInjection;

namespace WebSearchIndexing.Modules.Identity.Ui.Services;

public class SmartHttpClientInterceptor : DelegatingHandler
{
    private readonly ISecureTokenStorage _tokenStorage;
    private readonly IServiceProvider _serviceProvider;

    public SmartHttpClientInterceptor(ISecureTokenStorage tokenStorage, IServiceProvider serviceProvider)
    {
        _tokenStorage = tokenStorage;
        _serviceProvider = serviceProvider;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        // Auto-attach access token to requests
        await AttachTokenIfAvailable(request);

        var response = await base.SendAsync(request, cancellationToken);

        // If we get 401 and it's not an auth endpoint, try to refresh token
        if (response.StatusCode == HttpStatusCode.Unauthorized && !IsAuthEndpoint(request.RequestUri))
        {
            // Try to refresh token using a separate HTTP client to avoid recursion
            var refreshSuccess = await TryRefreshTokenAsync();
            
            if (refreshSuccess)
            {
                // Retry the original request with new token
                var newRequest = await CloneRequestAsync(request);
                await AttachTokenIfAvailable(newRequest);
                response = await base.SendAsync(newRequest, cancellationToken);
            }
        }

        return response;
    }

    private async Task AttachTokenIfAvailable(HttpRequestMessage request)
    {
        var token = await _tokenStorage.GetAccessTokenAsync();
        if (!string.IsNullOrEmpty(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
    }

    private async Task<bool> TryRefreshTokenAsync()
    {
        try
        {
            // Create a separate HttpClient for refresh to avoid circular dependency
            using var scope = _serviceProvider.CreateScope();
            var httpClientFactory = scope.ServiceProvider.GetRequiredService<IHttpClientFactory>();
            using var authClient = httpClientFactory.CreateClient("AuthApi");
            
            var response = await authClient.PostAsync("/api/v1/identity/auth/refresh", null);
            
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var refreshResponse = System.Text.Json.JsonSerializer.Deserialize<RefreshResponse>(content, new System.Text.Json.JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (refreshResponse?.AccessToken != null && refreshResponse.ExpiresAt.HasValue)
                {
                    // Update access token in memory
                    _tokenStorage.SetAccessToken(refreshResponse.AccessToken, refreshResponse.ExpiresAt.Value);
                    return true;
                }
            }

            return false;
        }
        catch
        {
            return false;
        }
    }

    private static bool IsAuthEndpoint(Uri? uri)
    {
        if (uri == null) return false;
        
        var path = uri.AbsolutePath.ToLowerInvariant();
        return path.Contains("/identity/auth/") || 
               path.EndsWith("/login") ||
               path.EndsWith("/register") ||
               path.EndsWith("/refresh") ||
               path.EndsWith("/logout");
    }

    private static async Task<HttpRequestMessage> CloneRequestAsync(HttpRequestMessage original)
    {
        var clone = new HttpRequestMessage(original.Method, original.RequestUri)
        {
            Version = original.Version
        };

        // Copy headers
        foreach (var header in original.Headers)
        {
            clone.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }

        // Copy content if present
        if (original.Content != null)
        {
            var content = await original.Content.ReadAsByteArrayAsync();
            clone.Content = new ByteArrayContent(content);

            // Copy content headers
            foreach (var header in original.Content.Headers)
            {
                clone.Content.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }
        }

        return clone;
    }

    private class RefreshResponse
    {
        public string? AccessToken { get; set; }
        public DateTime? ExpiresAt { get; set; }
    }
}
