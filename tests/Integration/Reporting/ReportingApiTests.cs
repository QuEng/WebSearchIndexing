using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using WebSearchIndexing.Modules.Reporting.Application.DTOs;
using Xunit;

namespace WebSearchIndexing.Tests.Integration.Reporting;

public class ReportingApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public ReportingApiTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task GetDashboardStats_ReturnsValidResponse()
    {
        // Act
        var response = await _client.GetAsync("/api/reporting/dashboard");
        
        // Assert
        response.EnsureSuccessStatusCode();
        var stats = await response.Content.ReadFromJsonAsync<DashboardStatsDto>();
        
        Assert.NotNull(stats);
        Assert.NotNull(stats.ServiceAccounts);
        Assert.NotNull(stats.Quota);
        Assert.NotNull(stats.PendingRequests);
        Assert.NotNull(stats.CompletedRequests);
        Assert.NotNull(stats.CompletedRequestsToday);
    }

    [Fact]
    public async Task GetPeriodStats_ReturnsValidResponse()
    {
        // Arrange
        var from = DateTime.UtcNow.AddDays(-7).ToString("yyyy-MM-dd");
        var to = DateTime.UtcNow.ToString("yyyy-MM-dd");
        
        // Act
        var response = await _client.GetAsync($"/api/reporting/period?from={from}&to={to}");
        
        // Assert
        response.EnsureSuccessStatusCode();
        var stats = await response.Content.ReadFromJsonAsync<PeriodStatsDto>();
        
        Assert.NotNull(stats);
        Assert.NotNull(stats.CompletedRequests);
        Assert.NotNull(stats.FailedRequests);
    }

    [Fact]
    public async Task GetQuotaUsage_ReturnsValidResponse()
    {
        // Act
        var response = await _client.GetAsync("/api/reporting/quota");
        
        // Assert
        response.EnsureSuccessStatusCode();
        var stats = await response.Content.ReadFromJsonAsync<QuotaUsageDto>();
        
        Assert.NotNull(stats);
        Assert.True(stats.TotalQuota >= 0);
        Assert.True(stats.UsedQuota >= 0);
        Assert.True(stats.AvailableQuota >= 0);
    }
}
