using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using WebSearchIndexing.Modules.Catalog.Application.DTOs;
using WebSearchIndexing.Modules.Core.Application.DTOs;

namespace WebSearchIndexing.Modules.Core.Ui.Services;

public interface ICoreHttpClient
{
    Task<SettingsDto?> GetSettingsAsync();
    Task<SettingsDto?> UpdateSettingsAsync(int requestsPerDay, bool? isEnabled = null);
    Task<List<ServiceAccountDto>?> GetServiceAccountsAsync();
}

public class CoreHttpClient : ICoreHttpClient
{
    private readonly HttpClient _httpClient;
    private readonly NavigationManager _navigationManager;
    private readonly ILogger<CoreHttpClient> _logger;

    public CoreHttpClient(
        HttpClient httpClient, 
        NavigationManager navigationManager,
        ILogger<CoreHttpClient> logger)
    {
        _httpClient = httpClient;
        _navigationManager = navigationManager;
        _logger = logger;
        
        if (string.IsNullOrEmpty(_httpClient.BaseAddress?.ToString()))
        {
            var baseAddress = _navigationManager.BaseUri;
            _httpClient.BaseAddress = new Uri(baseAddress);
            _logger.LogInformation("Set HttpClient BaseAddress to {BaseAddress}", baseAddress);
        }
    }

    public async Task<SettingsDto?> GetSettingsAsync()
    {
        try
        {
            _logger.LogInformation("Getting settings from API");
            return await _httpClient.GetFromJsonAsync<SettingsDto>("/api/core/settings");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get settings");
            return null;
        }
    }

    public async Task<SettingsDto?> UpdateSettingsAsync(int requestsPerDay, bool? isEnabled = null)
    {
        try
        {
            _logger.LogInformation("Updating settings via API");
            var request = new { 
                RequestsPerDay = requestsPerDay, 
                IsEnabled = isEnabled 
            };
            var response = await _httpClient.PutAsJsonAsync("/api/core/settings", request);
            return response.IsSuccessStatusCode 
                ? await response.Content.ReadFromJsonAsync<SettingsDto>() 
                : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update settings");
            return null;
        }
    }

    public async Task<List<ServiceAccountDto>?> GetServiceAccountsAsync()
    {
        try
        {
            _logger.LogInformation("Getting service accounts from API");
            return await _httpClient.GetFromJsonAsync<List<ServiceAccountDto>>("/api/catalog/service-accounts");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get service accounts");
            return null;
        }
    }
}
