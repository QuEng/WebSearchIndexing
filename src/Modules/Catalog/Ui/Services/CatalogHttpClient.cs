using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using WebSearchIndexing.Modules.Catalog.Application.DTOs;

namespace WebSearchIndexing.Modules.Catalog.Ui.Services;

public interface ICatalogHttpClient
{
    Task<List<ServiceAccountDto>?> GetServiceAccountsAsync();
    Task<ServiceAccountDto?> GetServiceAccountByIdAsync(Guid id);
    Task<ServiceAccountDto?> AddServiceAccountAsync(object serviceAccountRequest);
    Task<ServiceAccountDto?> UpdateServiceAccountAsync(Guid id, uint quotaLimitPerDay);
    Task<bool> DeleteServiceAccountAsync(Guid id);
    Task<bool> CheckServiceAccountExistsAsync(string projectId);
}

public class CatalogHttpClient : ICatalogHttpClient
{
    private readonly HttpClient _httpClient;
    private readonly NavigationManager _navigationManager;
    private readonly ILogger<CatalogHttpClient> _logger;

    public CatalogHttpClient(
        HttpClient httpClient, 
        NavigationManager navigationManager,
        ILogger<CatalogHttpClient> logger)
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

    public async Task<List<ServiceAccountDto>?> GetServiceAccountsAsync()
    {
        try
        {
            _logger.LogInformation("Getting service accounts from API");
            return await _httpClient.GetFromJsonAsync<List<ServiceAccountDto>>("/api/v1/catalog/service-accounts");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get service accounts");
            return null;
        }
    }

    public async Task<ServiceAccountDto?> GetServiceAccountByIdAsync(Guid id)
    {
        try
        {
            _logger.LogInformation("Getting service account {Id} from API", id);
            return await _httpClient.GetFromJsonAsync<ServiceAccountDto>($"/api/v1/catalog/service-accounts/{id}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get service account {Id}", id);
            return null;
        }
    }

    public async Task<ServiceAccountDto?> AddServiceAccountAsync(object serviceAccountRequest)
    {
        try
        {
            _logger.LogInformation("Adding service account via API");
            var response = await _httpClient.PostAsJsonAsync("/api/v1/catalog/service-accounts", serviceAccountRequest);
            return response.IsSuccessStatusCode 
                ? await response.Content.ReadFromJsonAsync<ServiceAccountDto>() 
                : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to add service account");
            return null;
        }
    }

    public async Task<ServiceAccountDto?> UpdateServiceAccountAsync(Guid id, uint quotaLimitPerDay)
    {
        try
        {
            _logger.LogInformation("Updating service account {Id} via API", id);
            var request = new { QuotaLimitPerDay = quotaLimitPerDay };
            var response = await _httpClient.PutAsJsonAsync($"/api/v1/catalog/service-accounts/{id}", request);
            return response.IsSuccessStatusCode 
                ? await response.Content.ReadFromJsonAsync<ServiceAccountDto>() 
                : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update service account {Id}", id);
            return null;
        }
    }

    public async Task<bool> DeleteServiceAccountAsync(Guid id)
    {
        try
        {
            _logger.LogInformation("Deleting service account {Id} via API", id);
            var response = await _httpClient.DeleteAsync($"/api/v1/catalog/service-accounts/{id}");
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete service account {Id}", id);
            return false;
        }
    }

    public async Task<bool> CheckServiceAccountExistsAsync(string projectId)
    {
        try
        {
            _logger.LogInformation("Checking if service account exists for project {ProjectId} via API", projectId);
            var response = await _httpClient.GetFromJsonAsync<dynamic>($"/api/v1/catalog/service-accounts/exists/{projectId}");
            return response?.exists == true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to check service account existence for project {ProjectId}", projectId);
            return false;
        }
    }
}
