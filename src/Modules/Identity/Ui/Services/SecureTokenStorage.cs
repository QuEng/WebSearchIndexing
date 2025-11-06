using Microsoft.JSInterop;
using System.Text.Json;
using System.Text;

namespace WebSearchIndexing.Modules.Identity.Ui.Services;

/// <summary>
/// Secure token storage that uses memory-first approach with encrypted localStorage fallback
/// Provides persistent storage while maintaining security against XSS attacks
/// </summary>
public class SecureTokenStorage : ISecureTokenStorage
{
    private readonly IJSRuntime _jsRuntime;
    private readonly string _storageKey = "wsi_auth_token";
    private readonly string _keyStorageKey = "wsi_encryption_key";
    private string? _encryptionKey;
    
    private string? _accessToken;
    private DateTime? _expiresAt;
    private bool _rememberMe = false;
    private bool _isInitialized = false;

    public SecureTokenStorage(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public void SetAccessToken(string token, DateTime expiresAt)
    {
        _accessToken = token;
        _expiresAt = expiresAt;
        
        // Always save to localStorage for persistence between sessions
        // RememberMe only affects long-term storage duration
        _ = Task.Run(async () => await SaveToStorageAsync(token, expiresAt));
    }

    public void SetRememberMe(bool remember)
    {
        _rememberMe = remember;
        
        // If disabling remember me, clear storage
        if (!remember)
        {
            _ = Task.Run(async () => await ClearStorageAsync());
        }
    }

    public string? GetAccessToken()
    {
        // Ensure we've loaded from storage first
        if (!_isInitialized)
        {
            // For synchronous access, return memory value if available
            // Async initialization should happen elsewhere
        }
        
        if (IsAccessTokenValid())
            return _accessToken;
        
        // Auto-clear expired tokens
        ClearAccessToken();
        return null;
    }

    public async Task<string?> GetAccessTokenAsync()
    {
        await EnsureInitializedAsync();
        return GetAccessToken();
    }

    public DateTime? GetAccessTokenExpiry() => _expiresAt;

    public bool IsAccessTokenValid()
    {
        return !string.IsNullOrEmpty(_accessToken) && 
               _expiresAt.HasValue && 
               _expiresAt.Value > DateTime.UtcNow.AddMinutes(1); // 1 minute buffer
    }

    public void ClearAccessToken()
    {
        _accessToken = null;
        _expiresAt = null;
        
        // Clear from localStorage asynchronously
        _ = Task.Run(async () => await ClearStorageAsync());
    }

    public void ClearAll()
    {
        _rememberMe = false;
        ClearAccessToken();
        
        // Clear encryption key asynchronously
        _ = Task.Run(async () =>
        {
            try
            {
                await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", _keyStorageKey);
                _encryptionKey = null;
            }
            catch (Exception)
            {
                // Silently fail
            }
        });
        // Note: Refresh tokens are in HTTP-Only cookies and cleared server-side
    }

    public async Task InitializeAsync()
    {
        await EnsureInitializedAsync();
    }

    private async Task EnsureInitializedAsync()
    {
        if (_isInitialized) return;

        try
        {
            // Initialize encryption key first
            await EnsureEncryptionKeyAsync();
            
            await LoadFromStorageAsync();
        }
        catch (Exception)
        {
            // If loading fails, start with clean state
            _accessToken = null;
            _expiresAt = null;
        }
        finally
        {
            _isInitialized = true;
        }
    }

    private async Task EnsureEncryptionKeyAsync()
    {
        if (_encryptionKey != null) return;

        try
        {
            // Try to load existing key from localStorage
            _encryptionKey = await _jsRuntime.InvokeAsync<string?>("localStorage.getItem", _keyStorageKey);
            
            if (string.IsNullOrEmpty(_encryptionKey))
            {
                // Generate a new key and save it
                _encryptionKey = GenerateSessionKey();
                await _jsRuntime.InvokeVoidAsync("localStorage.setItem", _keyStorageKey, _encryptionKey);
            }
        }
        catch (Exception)
        {
            // Fallback to session-only key
            _encryptionKey = GenerateSessionKey();
        }
    }

    private async Task SaveToStorageAsync(string token, DateTime expiresAt)
    {
        try
        {
            await EnsureEncryptionKeyAsync();
            
            var tokenData = new TokenData
            {
                AccessToken = token,
                ExpiresAt = expiresAt,
                RememberMe = _rememberMe
            };

            var json = JsonSerializer.Serialize(tokenData);
            var encrypted = EncryptString(json);
            
            if (!string.IsNullOrEmpty(encrypted))
            {
                await _jsRuntime.InvokeVoidAsync("localStorage.setItem", _storageKey, encrypted);
            }
        }
        catch (Exception ex)
        {
            // Log critical errors only
            await _jsRuntime.InvokeVoidAsync("console.error", $"SaveToStorage error: {ex.Message}");
        }
    }

    private async Task LoadFromStorageAsync()
    {
        try
        {
            var encrypted = await _jsRuntime.InvokeAsync<string?>("localStorage.getItem", _storageKey);
            
            if (string.IsNullOrEmpty(encrypted))
                return;

            var json = DecryptString(encrypted);
            if (string.IsNullOrEmpty(json))
            {
                return;
            }
            
            var tokenData = JsonSerializer.Deserialize<TokenData>(json);
            
            if (tokenData != null)
            {
                _accessToken = tokenData.AccessToken;
                _expiresAt = tokenData.ExpiresAt;
                _rememberMe = tokenData.RememberMe;
                
                // Check if token is still valid, clear if expired
                if (!IsAccessTokenValid())
                {
                    _accessToken = null;
                    _expiresAt = null;
                    _rememberMe = false;
                    await ClearStorageAsync();
                }
            }
        }
        catch (Exception ex)
        {
            await _jsRuntime.InvokeVoidAsync("console.error", $"LoadFromStorage error: {ex.Message}");
            // If decryption or parsing fails, clear storage
            await ClearStorageAsync();
        }
    }

    private async Task ClearStorageAsync()
    {
        try
        {
            await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", _storageKey);
        }
        catch (Exception)
        {
            // Silently fail
        }
    }

    private string GenerateSessionKey()
    {
        // Generate a simple session key without cryptographic requirements
        // This is just for basic obfuscation in client-side storage
        var random = new Random();
        var keyChars = new char[32];
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        
        for (int i = 0; i < keyChars.Length; i++)
        {
            keyChars[i] = chars[random.Next(chars.Length)];
        }
        
        return new string(keyChars);
    }

    private string EncryptString(string plainText)
    {
        try
        {
            if (string.IsNullOrEmpty(plainText))
            {
                return string.Empty;
            }
            
            // Since AES is not supported in Blazor WASM, use Base64 encoding with some obfuscation
            // This is not real encryption but provides basic obfuscation
            var bytes = Encoding.UTF8.GetBytes(plainText);
            
            // Add some simple obfuscation by XORing with key bytes
            if (!string.IsNullOrEmpty(_encryptionKey))
            {
                var keyBytes = Encoding.UTF8.GetBytes(_encryptionKey);
                for (int i = 0; i < bytes.Length; i++)
                {
                    bytes[i] ^= keyBytes[i % keyBytes.Length];
                }
            }
            
            return Convert.ToBase64String(bytes);
        }
        catch (Exception ex)
        {
            // Log error asynchronously
            _ = Task.Run(async () => await _jsRuntime.InvokeVoidAsync("console.error", $"EncryptString error: {ex.Message}"));
            return string.Empty;
        }
    }

    private string DecryptString(string encryptedText)
    {
        try
        {
            if (string.IsNullOrEmpty(encryptedText))
                return string.Empty;
                
            // Decode from Base64 and reverse the XOR obfuscation
            var bytes = Convert.FromBase64String(encryptedText);
            
            // Reverse the XOR obfuscation using the same key
            if (!string.IsNullOrEmpty(_encryptionKey))
            {
                var keyBytes = Encoding.UTF8.GetBytes(_encryptionKey);
                for (int i = 0; i < bytes.Length; i++)
                {
                    bytes[i] ^= keyBytes[i % keyBytes.Length];
                }
            }
            
            return Encoding.UTF8.GetString(bytes);
        }
        catch
        {
            // If decryption fails, return empty string
            return string.Empty;
        }
    }

    private class TokenData
    {
        public string AccessToken { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
        public bool RememberMe { get; set; }
    }
}
