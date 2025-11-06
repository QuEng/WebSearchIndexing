using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using WebSearchIndexing.Modules.Identity.Application.Caching;
using WebSearchIndexing.Modules.Identity.Application.Security;
using WebSearchIndexing.Modules.Identity.Infrastructure.Caching;
using Xunit;

namespace WebSearchIndexing.Tests.Unit.Identity.Caching;

public sealed class InMemoryCacheServiceTests
{
    private readonly Mock<ISecurityLoggingService> _securityLoggingMock;
    private readonly Mock<ILogger<InMemoryCacheService>> _loggerMock;
    private readonly InMemoryCacheService _cacheService;

    public InMemoryCacheServiceTests()
    {
        _securityLoggingMock = new Mock<ISecurityLoggingService>();
        _loggerMock = new Mock<ILogger<InMemoryCacheService>>();
        _cacheService = new InMemoryCacheService(_securityLoggingMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task GetOrCreateAsync_WithNewKey_CallsFactory()
    {
        // Arrange
        var key = "test:key";
        var expectedValue = "test-value";
        var factoryCalled = false;

        // Act
        var result = await _cacheService.GetOrCreateAsync(
            key,
            async () =>
            {
                factoryCalled = true;
                return await Task.FromResult(expectedValue);
            },
            TimeSpan.FromMinutes(5));

        // Assert
        result.Should().Be(expectedValue);
        factoryCalled.Should().BeTrue();
    }

    [Fact]
    public async Task GetOrCreateAsync_WithExistingKey_DoesNotCallFactory()
    {
        // Arrange
        var key = "test:key";
        var expectedValue = "test-value";
        var factoryCallCount = 0;

        // First call
        await _cacheService.GetOrCreateAsync(
            key,
            async () =>
            {
                factoryCallCount++;
                return await Task.FromResult(expectedValue);
            },
            TimeSpan.FromMinutes(5));

        // Act - Second call
        var result = await _cacheService.GetOrCreateAsync(
            key,
            async () =>
            {
                factoryCallCount++;
                return await Task.FromResult("should-not-be-called");
            },
            TimeSpan.FromMinutes(5));

        // Assert
        result.Should().Be(expectedValue);
        factoryCallCount.Should().Be(1); // Factory should only be called once
    }

    [Fact]
    public async Task GetAsync_WithExistingKey_ReturnsValue()
    {
        // Arrange
        var key = "test:key";
        var value = "test-value";
        await _cacheService.SetAsync(key, value, TimeSpan.FromMinutes(5));

        // Act
        var result = await _cacheService.GetAsync<string>(key);

        // Assert
        result.Should().Be(value);
    }

    [Fact]
    public async Task GetAsync_WithNonExistingKey_ReturnsDefault()
    {
        // Arrange
        var key = "non-existing-key";

        // Act
        var result = await _cacheService.GetAsync<string>(key);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task SetAsync_StoresValue()
    {
        // Arrange
        var key = "test:key";
        var value = "test-value";

        // Act
        await _cacheService.SetAsync(key, value, TimeSpan.FromMinutes(5));
        var result = await _cacheService.GetAsync<string>(key);

        // Assert
        result.Should().Be(value);
    }

    [Fact]
    public async Task RemoveAsync_RemovesValue()
    {
        // Arrange
        var key = "test:key";
        var value = "test-value";
        await _cacheService.SetAsync(key, value, TimeSpan.FromMinutes(5));

        // Act
        await _cacheService.RemoveAsync(key);
        var result = await _cacheService.GetAsync<string>(key);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task RemoveByPatternAsync_RemovesMatchingKeys()
    {
        // Arrange
        await _cacheService.SetAsync("user:123:entity", "value1", TimeSpan.FromMinutes(5));
        await _cacheService.SetAsync("user:123:roles", "value2", TimeSpan.FromMinutes(5));
        await _cacheService.SetAsync("user:456:entity", "value3", TimeSpan.FromMinutes(5));

        // Act
        await _cacheService.RemoveByPatternAsync("user:123:*");

        // Assert
        var result1 = await _cacheService.GetAsync<string>("user:123:entity");
        var result2 = await _cacheService.GetAsync<string>("user:123:roles");
        var result3 = await _cacheService.GetAsync<string>("user:456:entity");

        result1.Should().BeNull();
        result2.Should().BeNull();
        result3.Should().NotBeNull(); // Should not be removed
    }

    [Fact]
    public async Task InvalidateUserCacheAsync_RemovesUserSpecificKeys()
    {
        // Arrange
        var userId = Guid.NewGuid();
        await _cacheService.SetAsync($"user:{userId}:entity", "value1", TimeSpan.FromMinutes(5));
        await _cacheService.SetAsync($"user:{userId}:roles", "value2", TimeSpan.FromMinutes(5));
        await _cacheService.SetAsync($"user:{Guid.NewGuid()}:entity", "value3", TimeSpan.FromMinutes(5));

        // Act
        await _cacheService.InvalidateUserCacheAsync(userId, CacheInvalidationReason.UserLogout);

        // Assert
        var result1 = await _cacheService.GetAsync<string>($"user:{userId}:entity");
        var result2 = await _cacheService.GetAsync<string>($"user:{userId}:roles");

        result1.Should().BeNull();
        result2.Should().BeNull();
    }

    [Fact]
    public async Task InvalidateTenantCacheAsync_RemovesTenantSpecificKeys()
    {
        // Arrange
        var tenantId = "tenant-123";
        await _cacheService.SetAsync($"tenant:{tenantId}:users", "value1", TimeSpan.FromMinutes(5));
        await _cacheService.SetAsync($"tenant:{tenantId}:roles", "value2", TimeSpan.FromMinutes(5));
        await _cacheService.SetAsync("tenant:tenant-456:users", "value3", TimeSpan.FromMinutes(5));

        // Act
        await _cacheService.InvalidateTenantCacheAsync(tenantId, CacheInvalidationReason.PermissionChange);

        // Assert
        var result1 = await _cacheService.GetAsync<string>($"tenant:{tenantId}:users");
        var result2 = await _cacheService.GetAsync<string>($"tenant:{tenantId}:roles");
        var result3 = await _cacheService.GetAsync<string>("tenant:tenant-456:users");

        result1.Should().BeNull();
        result2.Should().BeNull();
        result3.Should().NotBeNull();
    }

    [Fact]
    public async Task GetStatisticsAsync_ReturnsCorrectCounts()
    {
        // Arrange
        await _cacheService.SetAsync("key1", "value1", TimeSpan.FromMinutes(5));
        await _cacheService.SetAsync("key2", "value2", TimeSpan.FromMinutes(5));
        
        // Generate cache hits
        await _cacheService.GetAsync<string>("key1");
        await _cacheService.GetAsync<string>("key1");
        
        // Generate cache miss
        await _cacheService.GetAsync<string>("non-existing");

        // Act
        var stats = await _cacheService.GetStatisticsAsync();

        // Assert
        stats.TotalEntries.Should().Be(2);
        stats.TotalHits.Should().Be(2);
        stats.TotalMisses.Should().Be(1);
    }

    [Fact]
    public async Task InvalidateUserCacheAsync_TracksInvalidationReason()
    {
        // Arrange
        var userId = Guid.NewGuid();
        await _cacheService.SetAsync($"user:{userId}:entity", "value", TimeSpan.FromMinutes(5));

        // Act
        await _cacheService.InvalidateUserCacheAsync(userId, CacheInvalidationReason.PasswordChange);
        var stats = await _cacheService.GetStatisticsAsync();

        // Assert
        stats.InvalidationsByReason.Should().ContainKey(CacheInvalidationReason.PasswordChange);
        stats.InvalidationsByReason[CacheInvalidationReason.PasswordChange].Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task InvalidateUserCacheAsync_LogsSecurityEvent()
    {
        // Arrange
        var userId = Guid.NewGuid();
        await _cacheService.SetAsync($"user:{userId}:entity", "value", TimeSpan.FromMinutes(5));

        // Act
        await _cacheService.InvalidateUserCacheAsync(userId, CacheInvalidationReason.PasswordChange);

        // Assert
        _securityLoggingMock.Verify(
            x => x.LogSecurityEventAsync(
                It.Is<string>(s => s.Contains("Cache invalidation")),
                "CacheInvalidation",
                userId,
                It.IsAny<string>(),
                It.Is<Dictionary<string, object>>(d => 
                    d.ContainsKey("Reason") && 
                    d.ContainsKey("Pattern")),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetOrCreateAsync_WithExpiredEntry_CallsFactory()
    {
        // Arrange
        var key = "test:key";
        var factoryCallCount = 0;

        // First call with very short expiration
        await _cacheService.GetOrCreateAsync(
            key,
            async () =>
            {
                factoryCallCount++;
                return await Task.FromResult("value1");
            },
            TimeSpan.FromMilliseconds(1));

        // Wait for expiration
        await Task.Delay(100);

        // Act - Second call after expiration
        var result = await _cacheService.GetOrCreateAsync(
            key,
            async () =>
            {
                factoryCallCount++;
                return await Task.FromResult("value2");
            },
            TimeSpan.FromMinutes(5));

        // Assert
        result.Should().Be("value2");
        factoryCallCount.Should().Be(2); // Factory should be called twice
    }

    [Fact]
    public async Task ConcurrentAccess_HandledCorrectly()
    {
        // Arrange
        var key = "concurrent:key";
        var factoryCallCount = 0;
        var tasks = new List<Task<string>>();

        // Act - Multiple concurrent requests
        for (int i = 0; i < 10; i++)
        {
            tasks.Add(_cacheService.GetOrCreateAsync(
                key,
                async () =>
                {
                    Interlocked.Increment(ref factoryCallCount);
                    await Task.Delay(10);
                    return "value";
                },
                TimeSpan.FromMinutes(5)));
        }

        var results = await Task.WhenAll(tasks);

        // Assert
        results.Should().AllBe("value");
        factoryCallCount.Should().BeGreaterOrEqualTo(1); // Factory should be called, but exact count depends on timing
    }

    [Fact]
    public async Task RemoveByPatternAsync_WithComplexPattern_RemovesCorrectKeys()
    {
        // Arrange
        await _cacheService.SetAsync("user:123:entity", "value1", TimeSpan.FromMinutes(5));
        await _cacheService.SetAsync("user:123:globalroles", "value2", TimeSpan.FromMinutes(5));
        await _cacheService.SetAsync("user:456:entity", "value3", TimeSpan.FromMinutes(5));
        await _cacheService.SetAsync("role:789:entity", "value4", TimeSpan.FromMinutes(5));

        // Act
        await _cacheService.RemoveByPatternAsync("user:*");

        // Assert
        var userResult1 = await _cacheService.GetAsync<string>("user:123:entity");
        var userResult2 = await _cacheService.GetAsync<string>("user:123:globalroles");
        var userResult3 = await _cacheService.GetAsync<string>("user:456:entity");
        var roleResult = await _cacheService.GetAsync<string>("role:789:entity");

        userResult1.Should().BeNull();
        userResult2.Should().BeNull();
        userResult3.Should().BeNull();
        roleResult.Should().NotBeNull(); // Role key should remain
    }
}
