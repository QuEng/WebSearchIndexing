using System.Net.Http.Json;
using WebSearchIndexing.Contracts.Catalog;

namespace WebSearchIndexing.Modules.Catalog.Ui.Services;

public class ServiceAccountsApiService : IServiceAccountsApiService
{
    private readonly HttpClient _httpClient;
    private const string BaseUrl = "/api/v1/catalog/service-accounts";

    public ServiceAccountsApiService(HttpClient httpClient)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    public async Task<IEnumerable<ServiceAccountDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync(BaseUrl, cancellationToken);
        response.EnsureSuccessStatusCode();
        
        var result = await response.Content.ReadFromJsonAsync<IEnumerable<ServiceAccountDto>>(cancellationToken);
        return result ?? Enumerable.Empty<ServiceAccountDto>();
    }

    public async Task<ServiceAccountDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync($"{BaseUrl}/{id}", cancellationToken);
        
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            return null;
            
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<ServiceAccountDto>(cancellationToken);
    }

    public async Task<ServiceAccountDto> AddAsync(AddServiceAccountRequest request, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PostAsJsonAsync(BaseUrl, request, cancellationToken);
        response.EnsureSuccessStatusCode();
        
        var result = await response.Content.ReadFromJsonAsync<ServiceAccountDto>(cancellationToken);
        return result ?? throw new InvalidOperationException("Failed to deserialize response");
    }

    public async Task<ServiceAccountDto> UpdateAsync(Guid id, UpdateServiceAccountRequest request, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PutAsJsonAsync($"{BaseUrl}/{id}", request, cancellationToken);
        response.EnsureSuccessStatusCode();
        
        var result = await response.Content.ReadFromJsonAsync<ServiceAccountDto>(cancellationToken);
        return result ?? throw new InvalidOperationException("Failed to deserialize response");
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.DeleteAsync($"{BaseUrl}/{id}", cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public async Task<bool> ExistsAsync(string projectId, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync($"{BaseUrl}/exists/{projectId}", cancellationToken);
        response.EnsureSuccessStatusCode();
        
        var result = await response.Content.ReadFromJsonAsync<ExistsResponse>(cancellationToken);
        return result?.Exists ?? false;
    }

    private record ExistsResponse(bool Exists);
}
