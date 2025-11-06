using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using WebSearchIndexing.Modules.Identity.Application.Security;
using WebSearchIndexing.Modules.Identity.Infrastructure.Security;
using Xunit;

namespace WebSearchIndexing.Tests.Unit.Identity.Security;

public sealed class CookieSecurityValidatorTests
{
    private readonly Mock<ILogger<CookieSecurityValidator>> _loggerMock;
    private readonly CookieSecurityOptions _options;
    private readonly CookieSecurityValidator _validator;

    public CookieSecurityValidatorTests()
    {
        _loggerMock = new Mock<ILogger<CookieSecurityValidator>>();
        _options = new CookieSecurityOptions
        {
            RequireHttps = true,
            HttpOnly = true,
            SameSite = SameSiteMode.Strict,
            ExpirationMinutes = 60
        };

        _validator = new CookieSecurityValidator(Options.Create(_options), _loggerMock.Object);
    }

    [Fact]
    public async Task ValidateCookieSettings_WithValidSecureSettings_ReturnsSuccess()
    {
        // Arrange
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict
        };

        // Act
        var result = await _validator.ValidateCookieSettingsAsync(cookieOptions);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateCookieSettings_WithoutHttpOnly_ReturnsFailure()
    {
        // Arrange
        var cookieOptions = new CookieOptions
        {
            HttpOnly = false,
            Secure = true,
            SameSite = SameSiteMode.Strict
        };

        // Act
        var result = await _validator.ValidateCookieSettingsAsync(cookieOptions);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("HttpOnly must be enabled");
    }

    [Fact]
    public async Task ValidateCookieSettings_WithoutSecure_ReturnsFailure()
    {
        // Arrange
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = false,
            SameSite = SameSiteMode.Strict
        };

        // Act
        var result = await _validator.ValidateCookieSettingsAsync(cookieOptions);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Secure flag must be set");
    }

    [Theory]
    [InlineData(SameSiteMode.None)]
    public async Task ValidateCookieSettings_WithInsecureSameSite_ReturnsFailure(SameSiteMode sameSite)
    {
        // Arrange
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = sameSite
        };

        // Act
        var result = await _validator.ValidateCookieSettingsAsync(cookieOptions);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("SameSite must be set to Strict or Lax");
    }

    [Theory]
    [InlineData(SameSiteMode.Strict)]
    [InlineData(SameSiteMode.Lax)]
    public async Task ValidateCookieSettings_WithSecureSameSite_ReturnsSuccess(SameSiteMode sameSite)
    {
        // Arrange
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = sameSite
        };

        // Act
        var result = await _validator.ValidateCookieSettingsAsync(cookieOptions);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task IsRequestSecure_WithHttpsScheme_ReturnsTrue()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Scheme = "https";

        // Act
        var result = await _validator.IsRequestSecureAsync(context);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsRequestSecure_WithHttpScheme_ReturnsFalse()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Scheme = "http";

        // Act
        var result = await _validator.IsRequestSecureAsync(context);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task ValidateSameSitePolicy_WithMatchingOrigins_ReturnsTrue()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Headers["Origin"] = "https://example.com";
        context.Request.Host = new HostString("example.com");

        // Act
        var result = await _validator.ValidateSameSitePolicyAsync(context, SameSiteMode.Strict);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateSameSitePolicy_WithDifferentOrigins_InStrictMode_ReturnsFalse()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Headers["Origin"] = "https://attacker.com";
        context.Request.Host = new HostString("example.com");

        // Act
        var result = await _validator.ValidateSameSitePolicyAsync(context, SameSiteMode.Strict);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task ValidateSameSitePolicy_WithDifferentOrigins_InLaxMode_ReturnsTrue()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Headers["Origin"] = "https://attacker.com";
        context.Request.Host = new HostString("example.com");

        // Act
        var result = await _validator.ValidateSameSitePolicyAsync(context, SameSiteMode.Lax);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateSameSitePolicy_WithNoOriginHeader_ReturnsTrue()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Host = new HostString("example.com");

        // Act
        var result = await _validator.ValidateSameSitePolicyAsync(context, SameSiteMode.Strict);

        // Assert
        result.Should().BeTrue(); // No cross-site request
    }

    [Fact]
    public async Task ValidateCookieSettings_LogsWarnings_ForInsecureSettings()
    {
        // Arrange
        var cookieOptions = new CookieOptions
        {
            HttpOnly = false,
            Secure = false,
            SameSite = SameSiteMode.None
        };

        // Act
        await _validator.ValidateCookieSettingsAsync(cookieOptions);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
            Times.AtLeastOnce);
    }
}
