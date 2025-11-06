using Microsoft.AspNetCore.Components.Authorization;
using WebSearchIndexing.Modules.Identity.Ui.Models;
using WebSearchIndexing.Modules.Identity.Ui.Services;

namespace WebSearchIndexing.Modules.Identity.Ui.State;

public class IdentityStateManager : IDisposable
{
    private readonly AuthenticationStateProvider _authStateProvider;
    private readonly HybridAuthService _authService;
    private readonly HttpClient _httpClient;
    private readonly ISecureTokenStorage _tokenStorage;
    
    public UserState User { get; }
    public TenantState Tenant { get; }
    public AuthenticationState Authentication { get; }
    public ConnectionState Connection { get; }
    
    public IdentityStateManager(
        ISecureTokenStorage tokenStorage,
        AuthenticationStateProvider authStateProvider,
        HybridAuthService authService,
        HttpClient httpClient)
    {
        _authStateProvider = authStateProvider;
        _authService = authService;
        _httpClient = httpClient;
        _tokenStorage = tokenStorage;
        
        User = new UserState();
        Tenant = new TenantState();
        Authentication = new AuthenticationState(tokenStorage);
        Connection = new ConnectionState();
        
        // Subscribe to authentication changes
        _authStateProvider.AuthenticationStateChanged += OnAuthenticationStateChanged;
    }
    
    public event Action? StateChanged;
    
    public async Task InitializeAsync()
    {
        // Initialize token storage first
        await _tokenStorage.InitializeAsync();
        
        var authState = await _authStateProvider.GetAuthenticationStateAsync();
        
        if (authState.User.Identity?.IsAuthenticated == true)
        {
            await LoadUserDataAsync();
        }
    }
    
    public async Task LoadUserDataAsync()
    {
        try
        {
            Connection.SetConnected();
            
            // Load user profile
            await LoadUserProfileAsync();
            
            // Load available tenants
            await LoadUserTenantsAsync();
            
            NotifyStateChanged();
        }
        catch (Exception ex)
        {
            Connection.SetDisconnected(ex.Message);
            throw;
        }
    }
    
    private async Task LoadUserProfileAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("/api/v1/identity/users/profile");
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var profile = System.Text.Json.JsonSerializer.Deserialize<UserProfileModel>(content, new System.Text.Json.JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                
                if (profile != null)
                {
                    User.UpdateFromProfile(profile);
                }
            }
        }
        catch (Exception ex)
        {
            Connection.SetDisconnected($"Failed to load user profile: {ex.Message}");
        }
    }
    
    private async Task LoadUserTenantsAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("/api/v1/identity/tenants");
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var tenants = System.Text.Json.JsonSerializer.Deserialize<List<TenantInfo>>(content, new System.Text.Json.JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }) ?? new List<TenantInfo>();
                
                Tenant.UpdateAvailableTenants(tenants);
                
                // Auto-select first tenant if none selected
                if (!Tenant.HasSelectedTenant && tenants.Any())
                {
                    Tenant.SelectTenant(tenants.First());
                }
            }
        }
        catch (Exception ex)
        {
            Connection.SetDisconnected($"Failed to load tenants: {ex.Message}");
        }
    }
    
    public async Task SelectTenantAsync(TenantInfo tenant)
    {
        Tenant.SelectTenant(tenant);
        NotifyStateChanged();
        
        // Could trigger tenant-specific data loading here
        await Task.CompletedTask;
    }
    
    public async Task LogoutAsync()
    {
        await _authService.LogoutAsync();
        ClearState();
        NotifyStateChanged();
    }
    
    private void ClearState()
    {
        User.Clear();
        Tenant.Clear();
        Connection.Reset();
    }
    
    private async void OnAuthenticationStateChanged(Task<Microsoft.AspNetCore.Components.Authorization.AuthenticationState> task)
    {
        var authState = await task;
        
        if (authState.User.Identity?.IsAuthenticated == true)
        {
            await LoadUserDataAsync();
        }
        else
        {
            ClearState();
        }
        
        Authentication.NotifyStateChanged();
        NotifyStateChanged();
    }
    
    private void NotifyStateChanged()
    {
        StateChanged?.Invoke();
    }
    
    public void Dispose()
    {
        _authStateProvider.AuthenticationStateChanged -= OnAuthenticationStateChanged;
    }
}
