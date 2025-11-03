using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using WebSearchIndexing.Modules.Reporting.Application.DTOs;

namespace WebSearchIndexing.Modules.Reporting.Ui.Services;

public interface IReportingHttpClient
{
    Task<DashboardStatsDto> GetDashboardStatsAsync(CancellationToken cancellationToken = default);
    Task<PeriodStatsDto> GetPeriodStatsAsync(DateTime from, DateTime to, CancellationToken cancellationToken = default);
    Task<QuotaUsageDto> GetQuotaUsageAsync(DateTime? date = null, CancellationToken cancellationToken = default);
}

internal sealed class ReportingHttpClient : IReportingHttpClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ReportingHttpClient> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public ReportingHttpClient(IHttpClientFactory httpClientFactory, NavigationManager navigationManager, ILogger<ReportingHttpClient> logger)
    {
        _httpClient = httpClientFactory.CreateClient("WebSearchIndexing.Api");
        _logger = logger;
        
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };
        
        // Configure base address for WASM compatibility
        ConfigureHttpClient(navigationManager);
    }

    private void ConfigureHttpClient(NavigationManager navigationManager)
    {
        // Set base address if not already set (for Blazor Server compatibility)
        if (_httpClient.BaseAddress == null)
        {
            _httpClient.BaseAddress = new Uri(navigationManager.BaseUri);
            _logger.LogDebug("HttpClient BaseAddress set to: {BaseAddress}", _httpClient.BaseAddress);
        }
        else
        {
            _logger.LogDebug("HttpClient BaseAddress already set to: {BaseAddress}", _httpClient.BaseAddress);
        }
        
        // Add common headers for API requests
        _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
    }

    public async Task<DashboardStatsDto> GetDashboardStatsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Making request to: {Url}", new Uri(_httpClient.BaseAddress!, "/api/v1/reporting/dashboard"));
            var response = await _httpClient.GetAsync("/api/v1/reporting/dashboard", cancellationToken);
            
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<DashboardStatsDto>(json, _jsonOptions) ?? new DashboardStatsDto();
            }
            
            _logger.LogWarning("Failed to get dashboard stats. Status: {StatusCode}", response.StatusCode);
            return new DashboardStatsDto();
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP request failed when calling dashboard API");
            return new DashboardStatsDto();
        }
        catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
        {
            _logger.LogWarning("Request to dashboard API timed out");
            return new DashboardStatsDto();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error when calling dashboard API");
            return new DashboardStatsDto();
        }
    }

    public async Task<PeriodStatsDto> GetPeriodStatsAsync(DateTime from, DateTime to, CancellationToken cancellationToken = default)
    {
        try
        {
            var fromStr = from.ToString("yyyy-MM-dd");
            var toStr = to.ToString("yyyy-MM-dd");
            var response = await _httpClient.GetAsync($"/api/v1/reporting/period?from={fromStr}&to={toStr}", cancellationToken);
            
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<PeriodStatsDto>(json, _jsonOptions) ?? new PeriodStatsDto();
            }
            
            _logger.LogWarning("Failed to get period stats. Status: {StatusCode}", response.StatusCode);
            return new PeriodStatsDto();
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP request failed when calling period stats API");
            return new PeriodStatsDto();
        }
        catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
        {
            _logger.LogWarning("Request to period stats API timed out");
            return new PeriodStatsDto();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error when calling period stats API");
            return new PeriodStatsDto();
        }
    }

    public async Task<QuotaUsageDto> GetQuotaUsageAsync(DateTime? date = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var url = "/api/v1/reporting/quota";
            if (date.HasValue)
            {
                var dateStr = date.Value.ToString("yyyy-MM-dd");
                url += $"?date={dateStr}";
            }

            var response = await _httpClient.GetAsync(url, cancellationToken);
            
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<QuotaUsageDto>(json, _jsonOptions) ?? new QuotaUsageDto();
            }
            
            _logger.LogWarning("Failed to get quota usage. Status: {StatusCode}", response.StatusCode);
            return new QuotaUsageDto();
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP request failed when calling quota usage API");
            return new QuotaUsageDto();
        }
        catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
        {
            _logger.LogWarning("Request to quota usage API timed out");
            return new QuotaUsageDto();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error when calling quota usage API");
            return new QuotaUsageDto();
        }
    }
}
