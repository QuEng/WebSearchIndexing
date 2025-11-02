using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace WebSearchIndexing.Tests.Integration.Submission;

public class SubmissionApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public SubmissionApiTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task GetSubmissions_ReturnsValidResponse()
    {
        // Act
        var response = await _client.GetAsync("/api/submission/submissions");
        
        // Assert
        response.EnsureSuccessStatusCode();
        // Note: Adjust based on actual DTO structure when available
    }

    [Fact]
    public async Task SubmitUrl_WithValidData_ReturnsSuccess()
    {
        // Arrange
        var submissionRequest = new
        {
            Url = "https://example.com/test-page",
            ServiceAccountId = Guid.NewGuid(),
            Priority = 1
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/submission/submit", submissionRequest);
        
        // Assert
        // Note: Adjust expected status code based on actual API behavior
        Assert.True(response.IsSuccessStatusCode);
    }
}
