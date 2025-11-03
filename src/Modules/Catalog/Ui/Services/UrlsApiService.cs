using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using WebSearchIndexing.Modules.Catalog.Application.DTOs;
using WebSearchIndexing.Modules.Catalog.Domain;

namespace WebSearchIndexing.Modules.Catalog.Ui.Services;

public sealed class UrlsApiService : IUrlsApiService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<UrlsApiService> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public UrlsApiService(IHttpClientFactory httpClientFactory, NavigationManager navigationManager, ILogger<UrlsApiService> logger)
    {
        _httpClient = httpClientFactory.CreateClient("WebSearchIndexing.Api");
        _logger = logger;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };

        // Configure base address for WASM compatibility
        if (string.IsNullOrEmpty(_httpClient.BaseAddress?.ToString()))
        {
            var baseAddress = navigationManager.BaseUri;
            _httpClient.BaseAddress = new Uri(baseAddress);
            _logger.LogInformation("Set HttpClient BaseAddress to {BaseAddress}", baseAddress);
        }
        
        // Add common headers for API requests
        _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
    }

    public async Task<IReadOnlyCollection<UrlItemDto>> GetUrlsAsync(
        int count = 10,
        int offset = 0,
        UrlItemStatus? status = null,
        UrlItemType? type = null,
        Guid? serviceAccountId = null,
        TimeSpan? subtractTime = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var queryParams = new List<string>
            {
                $"count={count}",
                $"offset={offset}"
            };

            if (status.HasValue)
                queryParams.Add($"status={status.Value:D}");

            if (type.HasValue)
                queryParams.Add($"type={type.Value:D}");

            if (serviceAccountId.HasValue)
                queryParams.Add($"serviceAccountId={serviceAccountId.Value}");

            if (subtractTime.HasValue)
                queryParams.Add($"subtractTimeHours={subtractTime.Value.TotalHours}");

            var queryString = string.Join("&", queryParams);
            _logger.LogInformation("Getting URLs from API with query: {QueryString}", queryString);
            
            var response = await _httpClient.GetAsync($"/api/v1/catalog/urls?{queryString}", cancellationToken);
            
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<UrlItemDto[]>(_jsonOptions, cancellationToken);
                return result ?? Array.Empty<UrlItemDto>();
            }
            
            _logger.LogWarning("Failed to get URLs. Status: {StatusCode}", response.StatusCode);
            return Array.Empty<UrlItemDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get URLs");
            return Array.Empty<UrlItemDto>();
        }
    }

    public async Task<int> GetUrlsCountAsync(
        UrlItemStatus? status = null,
        UrlItemType? type = null,
        Guid? serviceAccountId = null,
        TimeSpan? subtractTime = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var queryParams = new List<string>();

            if (status.HasValue)
                queryParams.Add($"status={status.Value:D}");

            if (type.HasValue)
                queryParams.Add($"type={type.Value:D}");

            if (serviceAccountId.HasValue)
                queryParams.Add($"serviceAccountId={serviceAccountId.Value}");

            if (subtractTime.HasValue)
                queryParams.Add($"subtractTimeHours={subtractTime.Value.TotalHours}");

            var queryString = queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : string.Empty;
            _logger.LogInformation("Getting URLs count from API with query: {QueryString}", queryString);
            
            var response = await _httpClient.GetAsync($"/api/v1/catalog/urls/count{queryString}", cancellationToken);
            
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<CountResponse>(_jsonOptions, cancellationToken);
                return result?.Count ?? 0;
            }
            
            _logger.LogWarning("Failed to get URLs count. Status: {StatusCode}", response.StatusCode);
            return 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get URLs count");
            return 0;
        }
    }

    public async Task<UrlItemDto> UpdateUrlAsync(
        Guid id,
        string url,
        UrlItemPriority priority,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var command = new
            {
                Id = id,
                Url = url,
                Priority = priority
            };

            _logger.LogInformation("Updating URL {Id} via API", id);
            
            var content = new StringContent(
                JsonSerializer.Serialize(command, _jsonOptions),
                Encoding.UTF8,
                "application/json");

            var response = await _httpClient.PutAsync($"/api/v1/catalog/urls/{id}", content, cancellationToken);
            
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<UrlItemDto>(_jsonOptions, cancellationToken);
                return result ?? throw new InvalidOperationException("Failed to deserialize update response.");
            }
            
            _logger.LogWarning("Failed to update URL {Id}. Status: {StatusCode}", id, response.StatusCode);
            throw new InvalidOperationException($"Failed to update URL. Status: {response.StatusCode}");
        }
        catch (Exception ex) when (ex is not InvalidOperationException)
        {
            _logger.LogError(ex, "Failed to update URL {Id}", id);
            throw new InvalidOperationException($"Failed to update URL: {ex.Message}", ex);
        }
    }

    public async Task<bool> DeleteUrlAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Deleting URL {Id} via API", id);
            var response = await _httpClient.DeleteAsync($"/api/v1/catalog/urls/{id}", cancellationToken);
            
            if (response.IsSuccessStatusCode)
            {
                return true;
            }
            
            _logger.LogWarning("Failed to delete URL {Id}. Status: {StatusCode}", id, response.StatusCode);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete URL {Id}", id);
            return false;
        }
    }

    public async Task<bool> DeleteUrlsBatchAsync(
        IReadOnlyCollection<Guid> ids,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var command = new { Ids = ids };
            
            _logger.LogInformation("Deleting {Count} URLs in batch via API", ids.Count);
            
            var content = new StringContent(
                JsonSerializer.Serialize(command, _jsonOptions),
                Encoding.UTF8,
                "application/json");

            var response = await _httpClient.SendAsync(
                new HttpRequestMessage(HttpMethod.Delete, "/api/v1/catalog/urls/batch")
                {
                    Content = content
                },
                cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                return true;
            }
            
            _logger.LogWarning("Failed to delete URLs batch. Status: {StatusCode}", response.StatusCode);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete URLs batch");
            return false;
        }
    }

    public async Task<IReadOnlyCollection<UrlItemDto>> ImportUrlsAsync(
        IReadOnlyCollection<ImportUrlEntry> urls,
        Guid? siteId = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var command = new
            {
                SiteId = siteId,
                Items = urls
            };

            _logger.LogInformation("Importing {Count} URLs via API", urls.Count);
            
            var content = new StringContent(
                JsonSerializer.Serialize(command, _jsonOptions),
                Encoding.UTF8,
                "application/json");

            var response = await _httpClient.PostAsync("/api/v1/catalog/batches", content, cancellationToken);
            
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<UrlItemDto[]>(_jsonOptions, cancellationToken);
                return result ?? Array.Empty<UrlItemDto>();
            }
            
            _logger.LogWarning("Failed to import URLs. Status: {StatusCode}", response.StatusCode);
            return Array.Empty<UrlItemDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to import URLs");
            return Array.Empty<UrlItemDto>();
        }
    }

    private sealed record CountResponse(int Count);
}
