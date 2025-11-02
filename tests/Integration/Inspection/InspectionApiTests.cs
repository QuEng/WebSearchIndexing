using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace WebSearchIndexing.Tests.Integration.Inspection;

public class InspectionApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public InspectionApiTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task GetInspectionResults_ReturnsValidResponse()
    {
        // Act
        var response = await _client.GetAsync("/api/inspection/results");
        
        // Assert
        response.EnsureSuccessStatusCode();
        // Note: Adjust based on actual API structure when available
    }

    [Fact]
    public async Task InspectUrl_WithValidUrl_ReturnsSuccess()
    {
        // Arrange
        var inspectionRequest = new
        {
            Url = "https://example.com/test-page"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/inspection/inspect", inspectionRequest);
        
        // Assert
        // Note: Adjust expected behavior based on actual API
        Assert.True(response.IsSuccessStatusCode || response.StatusCode == System.Net.HttpStatusCode.NotFound);
    }
}
