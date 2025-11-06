using System.Security.Claims;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using WebSearchIndexing.Modules.Identity.Application.Security;
using WebSearchIndexing.Modules.Identity.Infrastructure.Security;
using Xunit;

namespace WebSearchIndexing.Tests.Unit.Identity.Security;

public sealed class TokenSecurityValidatorTests
{
    private readonly Mock<ISecurityLoggingService> _securityLoggingMock;
    private readonly Mock<ILogger<InMemoryTokenSecurityValidator>> _loggerMock;
    private readonly InMemoryTokenSecurityValidator _validator;

    public TokenSecurityValidatorTests()
    {
        _securityLoggingMock = new Mock<ISecurityLoggingService>();
        _loggerMock = new Mock<ILogger<InMemoryTokenSecurityValidator>>();
        _validator = new InMemoryTokenSecurityValidator(_securityLoggingMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task ValidateTokenClaims_WithValidClaims_ReturnsSuccess()
    {
        // Arrange
        var claims = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.Email, "user@example.com"),
            new Claim("jti", Guid.NewGuid().ToString())
        }));

        // Act
        var result = await _validator.ValidateTokenClaimsAsync(claims, "127.0.0.1", "Mozilla/5.0");

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateTokenClaims_WithoutNameIdentifier_ReturnsFailure()
    {
        // Arrange
        var claims = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.Email, "user@example.com"),
            new Claim("jti", Guid.NewGuid().ToString())
        }));

        // Act
        var result = await _validator.ValidateTokenClaimsAsync(claims, "127.0.0.1", "Mozilla/5.0");

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("User ID claim is missing");
    }

    [Fact]
    public async Task ValidateTokenClaims_WithoutEmail_ReturnsFailure()
    {
        // Arrange
        var claims = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
            new Claim("jti", Guid.NewGuid().ToString())
        }));

        // Act
        var result = await _validator.ValidateTokenClaimsAsync(claims, "127.0.0.1", "Mozilla/5.0");

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Email claim is missing");
    }

    [Fact]
    public async Task ValidateTokenClaims_WithoutJti_ReturnsFailure()
    {
        // Arrange
        var claims = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.Email, "user@example.com")
        }));

        // Act
        var result = await _validator.ValidateTokenClaimsAsync(claims, "127.0.0.1", "Mozilla/5.0");

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Token ID (jti) claim is missing");
    }

    [Theory]
    [InlineData("bot")]
    [InlineData("crawler")]
    [InlineData("spider")]
    [InlineData("scraper")]
    public async Task ValidateTokenClaims_WithSuspiciousUserAgent_ReturnsFailure(string suspiciousAgent)
    {
        // Arrange
        var claims = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.Email, "user@example.com"),
            new Claim("jti", Guid.NewGuid().ToString())
        }));

        var userAgent = $"Some{suspiciousAgent}UserAgent";

        // Act
        var result = await _validator.ValidateTokenClaimsAsync(claims, "127.0.0.1", userAgent);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Suspicious user agent detected");
    }

    [Fact]
    public async Task IsTokenReplayedAsync_WithNewToken_ReturnsFalse()
    {
        // Arrange
        var jti = Guid.NewGuid().ToString();

        // Act
        var isReplayed = await _validator.IsTokenReplayedAsync(jti);

        // Assert
        isReplayed.Should().BeFalse();
    }

    [Fact]
    public async Task IsTokenReplayedAsync_WithUsedToken_ReturnsTrue()
    {
        // Arrange
        var jti = Guid.NewGuid().ToString();
        var userId = Guid.NewGuid().ToString();

        // First use
        await _validator.MarkTokenAsUsedAsync(jti, userId, DateTime.UtcNow.AddMinutes(15));

        // Act - try to use again
        var isReplayed = await _validator.IsTokenReplayedAsync(jti);

        // Assert
        isReplayed.Should().BeTrue();
    }

    [Fact]
    public async Task MarkTokenAsUsedAsync_StoresTokenInformation()
    {
        // Arrange
        var jti = Guid.NewGuid().ToString();
        var userId = Guid.NewGuid().ToString();
        var expiry = DateTime.UtcNow.AddMinutes(15);

        // Act
        await _validator.MarkTokenAsUsedAsync(jti, userId, expiry);

        // Assert
        var isReplayed = await _validator.IsTokenReplayedAsync(jti);
        isReplayed.Should().BeTrue();
    }

    [Fact]
    public async Task MarkTokenAsUsedAsync_WithExpiredToken_DoesNotStore()
    {
        // Arrange
        var jti = Guid.NewGuid().ToString();
        var userId = Guid.NewGuid().ToString();
        var expiry = DateTime.UtcNow.AddMinutes(-5); // Already expired

        // Act
        await _validator.MarkTokenAsUsedAsync(jti, userId, expiry);

        // Assert
        var isReplayed = await _validator.IsTokenReplayedAsync(jti);
        isReplayed.Should().BeFalse(); // Should not be marked as used since it's expired
    }

    [Fact]
    public async Task ValidateTokenClaims_LogsSecurityEvent_OnFailure()
    {
        // Arrange
        var claims = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString())
            // Missing email and jti
        }));

        var ipAddress = "192.168.1.1";
        var userAgent = "TestAgent";

        // Act
        await _validator.ValidateTokenClaimsAsync(claims, ipAddress, userAgent);

        // Assert
        _securityLoggingMock.Verify(
            x => x.LogSecurityEventAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<Guid?>(),
                It.IsAny<string>(),
                It.Is<Dictionary<string, object>>(d => 
                    d.ContainsKey("IpAddress") && 
                    d.ContainsKey("UserAgent")),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task IsTokenReplayedAsync_LogsReplayAttempt()
    {
        // Arrange
        var jti = Guid.NewGuid().ToString();
        var userId = Guid.NewGuid().ToString();
        await _validator.MarkTokenAsUsedAsync(jti, userId, DateTime.UtcNow.AddMinutes(15));

        // Act
        await _validator.IsTokenReplayedAsync(jti);

        // Assert
        _securityLoggingMock.Verify(
            x => x.LogSecurityEventAsync(
                It.Is<string>(s => s.Contains("Token replay attempt detected")),
                "TokenReplay",
                null,
                It.Is<string>(s => s == "High"),
                It.Is<Dictionary<string, object>>(d => d.ContainsKey("TokenId")),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ValidateTokenClaims_WithEmptyIpAddress_ReturnsFailure()
    {
        // Arrange
        var claims = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.Email, "user@example.com"),
            new Claim("jti", Guid.NewGuid().ToString())
        }));

        // Act
        var result = await _validator.ValidateTokenClaimsAsync(claims, "", "Mozilla/5.0");

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("IP address is missing");
    }

    [Fact]
    public async Task ValidateTokenClaims_WithEmptyUserAgent_ReturnsFailure()
    {
        // Arrange
        var claims = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.Email, "user@example.com"),
            new Claim("jti", Guid.NewGuid().ToString())
        }));

        // Act
        var result = await _validator.ValidateTokenClaimsAsync(claims, "127.0.0.1", "");

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("User agent is missing");
    }
}
