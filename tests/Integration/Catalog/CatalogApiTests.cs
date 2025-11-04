using Microsoft.AspNetCore.Mvc.Testing;
using System.Net.Http.Json;
using WebSearchIndexing.Modules.Catalog.Application.Commands.ServiceAccounts;
using WebSearchIndexing.Contracts.Catalog;
using Xunit;

namespace WebSearchIndexing.Tests.Integration.Catalog;

public class CatalogApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public CatalogApiTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task GetServiceAccounts_ReturnsValidResponse()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/catalog/service-accounts");

        // Assert
        response.EnsureSuccessStatusCode();
        var serviceAccounts = await response.Content.ReadFromJsonAsync<List<ServiceAccountDto>>();

        Assert.NotNull(serviceAccounts);
    }

    [Fact]
    public async Task CreateServiceAccount_WithValidData_ReturnsCreatedResponse()
    {
        // Arrange
        var command = new AddServiceAccountCommand(
            ProjectId: "test-project-123",
            CredentialsJson: "{}",
            QuotaLimitPerDay: 1000
        );

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/catalog/service-accounts", command);

        // Assert
        response.EnsureSuccessStatusCode();
        var createdServiceAccount = await response.Content.ReadFromJsonAsync<ServiceAccountDto>();

        Assert.NotNull(createdServiceAccount);
        Assert.Equal(command.ProjectId, createdServiceAccount.ProjectId);
        Assert.Equal(command.QuotaLimitPerDay, createdServiceAccount.QuotaLimitPerDay);
    }
}
