using Microsoft.AspNetCore.Mvc.Testing;
using System.Net.Http.Json;
using WebSearchIndexing.Modules.Core.Api;
using WebSearchIndexing.Modules.Core.Application.DTOs;
using Xunit;

namespace WebSearchIndexing.Tests.Integration.Core;

public class CoreApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public CoreApiTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task GetSettings_ReturnsValidResponse()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/core/settings");
        
        // Assert
        response.EnsureSuccessStatusCode();
        var settings = await response.Content.ReadFromJsonAsync<SettingsDto>();
        
        Assert.NotNull(settings);
        Assert.True(settings.RequestsPerDay > 0);
    }

    [Fact]
    public async Task UpdateSettings_WithValidData_ReturnsSuccess()
    {
        // Arrange - First get current settings
        var getResponse = await _client.GetAsync("/api/v1/core/settings");
        getResponse.EnsureSuccessStatusCode();
        var currentSettings = await getResponse.Content.ReadFromJsonAsync<SettingsDto>();
        Assert.NotNull(currentSettings);

        // Modify settings
        var updatedSettings = new UpdateSettingsRequest(currentSettings.RequestsPerDay + 1, currentSettings.IsEnabled);
        // Act
        var response = await _client.PutAsJsonAsync("/api/v1/core/settings", updatedSettings);
        
        // Assert
        Assert.Equal(System.Net.HttpStatusCode.NoContent, response.StatusCode);
        
        // Verify the update
        var verifyResponse = await _client.GetAsync("/api/v1/core/settings");
        verifyResponse.EnsureSuccessStatusCode();
        var verifiedSettings = await verifyResponse.Content.ReadFromJsonAsync<SettingsDto>();
        
        Assert.NotNull(verifiedSettings);
        Assert.Equal(updatedSettings.RequestsPerDay, verifiedSettings.RequestsPerDay);
    }
}
