using System.Net.Http.Json;
using WebSearchIndexing.Modules.Core.Application.DTOs;
using WebSearchIndexing.Modules.Core.Ui.Contracts;
using WebSearchIndexing.Modules.Core.Ui.Models;

namespace WebSearchIndexing.Modules.Core.Ui.Services;

public class CoreApiService : ICoreApiService
{
    private readonly HttpClient _httpClient;
    private const string BaseUrl = "/api/v1/core";

    public CoreApiService(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient("WebSearchIndexing.Api");
    }

    public async Task<SettingsDto?> GetSettingsAsync(CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync($"{BaseUrl}/settings", cancellationToken);
        
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            return null;
            
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<SettingsDto>(cancellationToken);
    }

    public async Task<SettingsDto> UpdateSettingsAsync(UpdateSettingsRequest request, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PutAsJsonAsync($"{BaseUrl}/settings", request, cancellationToken);
        response.EnsureSuccessStatusCode();
        
        var result = await response.Content.ReadFromJsonAsync<SettingsDto>(cancellationToken);
        return result ?? throw new InvalidOperationException("Failed to deserialize response");
    }

    public async Task TriggerProcessingAsync(CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PostAsync($"{BaseUrl}/settings/trigger-processing", null, cancellationToken);
        response.EnsureSuccessStatusCode();
    }
}
