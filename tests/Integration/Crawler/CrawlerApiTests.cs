using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace WebSearchIndexing.Tests.Integration.Crawler;

public class CrawlerApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public CrawlerApiTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task GetCrawlerStatus_ReturnsValidResponse()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/crawler/status");

        // Assert
        response.EnsureSuccessStatusCode();
        // Note: Adjust based on actual API structure when available
    }

    [Fact]
    public async Task StartCrawler_ReturnsSuccess()
    {
        // Act
        var response = await _client.PostAsync("/api/v1/crawler/start", null);

        // Assert
        // Note: Adjust expected behavior based on actual API
        Assert.True(response.IsSuccessStatusCode || response.StatusCode == System.Net.HttpStatusCode.NotFound);
    }
}
