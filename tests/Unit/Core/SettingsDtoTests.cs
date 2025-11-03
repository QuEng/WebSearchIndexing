using FluentAssertions;
using WebSearchIndexing.Modules.Core.Application.DTOs;
using WebSearchIndexing.Modules.Core.Domain;
using Xunit;

namespace WebSearchIndexing.Tests.Unit.Core;

public class SettingsDtoTests
{
    [Fact]
    public void FromDomain_WithValidSettings_ShouldMapCorrectly()
    {
        // Arrange
        var settings = new Settings
        {
            Id = Guid.NewGuid(),
            TenantId = Guid.NewGuid(),
            Key = "test-key",
            RequestsPerDay = 1000,
            IsEnabled = true
        };

        // Act
        var dto = SettingsDto.FromDomain(settings);

        // Assert
        dto.Should().NotBeNull();
        dto.Id.Should().Be(settings.Id);
        dto.TenantId.Should().Be(settings.TenantId);
        dto.Key.Should().Be(settings.Key);
        dto.RequestsPerDay.Should().Be(settings.RequestsPerDay);
        dto.IsEnabled.Should().Be(settings.IsEnabled);
    }

    [Fact]
    public void FromDomain_WithNullSettings_ShouldThrowException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => SettingsDto.FromDomain(null!));
    }

    [Fact]
    public void SettingsDto_WithInitialization_ShouldSetPropertiesCorrectly()
    {
        // Arrange
        var id = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var key = "test-key";
        var requestsPerDay = 500;
        var isEnabled = false;

        // Act
        var dto = new SettingsDto
        {
            Id = id,
            TenantId = tenantId,
            Key = key,
            RequestsPerDay = requestsPerDay,
            IsEnabled = isEnabled
        };

        // Assert
        dto.Id.Should().Be(id);
        dto.TenantId.Should().Be(tenantId);
        dto.Key.Should().Be(key);
        dto.RequestsPerDay.Should().Be(requestsPerDay);
        dto.IsEnabled.Should().Be(isEnabled);
    }
}
