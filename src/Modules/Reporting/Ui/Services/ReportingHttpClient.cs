using System.Net.Http.Json;
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

    public ReportingHttpClient(HttpClient httpClient, NavigationManager navigationManager, ILogger<ReportingHttpClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        
        // Set base address if not already set (for Blazor Server)
        if (_httpClient.BaseAddress == null)
        {
            _httpClient.BaseAddress = new Uri(navigationManager.BaseUri);
            _logger.LogDebug("HttpClient BaseAddress set to: {BaseAddress}", _httpClient.BaseAddress);
        }
        else
        {
            _logger.LogDebug("HttpClient BaseAddress already set to: {BaseAddress}", _httpClient.BaseAddress);
        }
    }

    public async Task<DashboardStatsDto> GetDashboardStatsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Making request to: {Url}", new Uri(_httpClient.BaseAddress!, "/api/v1/reporting/dashboard"));
            var result = await _httpClient.GetFromJsonAsync<DashboardStatsDto>("/api/v1/reporting/dashboard", cancellationToken);
            return result ?? new DashboardStatsDto();
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
    }

    public async Task<PeriodStatsDto> GetPeriodStatsAsync(DateTime from, DateTime to, CancellationToken cancellationToken = default)
    {
        try
        {
            var fromStr = from.ToString("yyyy-MM-dd");
            var toStr = to.ToString("yyyy-MM-dd");
            var result = await _httpClient.GetFromJsonAsync<PeriodStatsDto>($"/api/v1/reporting/period?from={fromStr}&to={toStr}", cancellationToken);
            return result ?? new PeriodStatsDto();
        }
        catch (HttpRequestException)
        {
            return new PeriodStatsDto();
        }
        catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
        {
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

            var result = await _httpClient.GetFromJsonAsync<QuotaUsageDto>(url, cancellationToken);
            return result ?? new QuotaUsageDto();
        }
        catch (HttpRequestException)
        {
            return new QuotaUsageDto();
        }
        catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
        {
            return new QuotaUsageDto();
        }
    }
}
