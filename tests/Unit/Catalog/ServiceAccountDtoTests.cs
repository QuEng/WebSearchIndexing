using WebSearchIndexing.Modules.Catalog.Application.DTOs;
using WebSearchIndexing.Modules.Catalog.Domain;
using Xunit;

namespace WebSearchIndexing.Tests.Unit.Catalog;

public class ServiceAccountDtoTests
{
    [Fact]
    public void FromDomain_WithValidServiceAccount_ShouldMapCorrectly()
    {
        // Arrange
        var serviceAccount = new ServiceAccount("test-project-123", "{\"type\":\"service_account\"}", 1000);
        serviceAccount.LoadQuotaUsage(250);

        // Act
        var dto = ServiceAccountDto.FromDomain(serviceAccount);

        // Assert
        Assert.NotNull(dto);
        Assert.Equal(serviceAccount.Id, dto.Id);
        Assert.Equal(serviceAccount.ProjectId, dto.ProjectId);
        Assert.Equal(serviceAccount.QuotaLimitPerDay, dto.QuotaLimitPerDay);
        Assert.Equal(serviceAccount.QuotaUsedInPeriod, dto.QuotaUsedInPeriod);
        Assert.Equal(serviceAccount.CreatedAt, dto.CreatedAt);
        Assert.Equal(serviceAccount.DeletedAt, dto.DeletedAt);
    }

    [Fact]
    public void FromDomain_WithNullServiceAccount_ShouldThrowException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => ServiceAccountDto.FromDomain(null!));
    }
}
