using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using WebSearchIndexing.Modules.Identity.Application.Abstractions;
using WebSearchIndexing.Modules.Identity.Application.Security;
using WebSearchIndexing.Modules.Identity.Domain.Entities;
using WebSearchIndexing.Modules.Identity.Infrastructure.Persistence;
using WebSearchIndexing.Modules.Identity.Infrastructure.Security;
using Xunit;

namespace WebSearchIndexing.Tests.Unit.Identity.Security;

public sealed class PasswordPolicyServiceTests : IDisposable
{
    private readonly IdentityDbContext _dbContext;
    private readonly Mock<IPasswordHasher> _passwordHasherMock;
    private readonly Mock<ILogger<PasswordPolicyService>> _loggerMock;
    private readonly PasswordPolicyOptions _options;
    private readonly PasswordPolicyService _service;

    public PasswordPolicyServiceTests()
    {
        var options = new DbContextOptionsBuilder<IdentityDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _dbContext = new IdentityDbContext(options);
        _passwordHasherMock = new Mock<IPasswordHasher>();
        _loggerMock = new Mock<ILogger<PasswordPolicyService>>();

        _options = new PasswordPolicyOptions
        {
            MinLength = 8,
            MaxLength = 128,
            RequireUppercase = true,
            RequireLowercase = true,
            RequireDigit = true,
            RequireSpecialCharacter = true,
            PasswordHistoryLimit = 5,
            MinPasswordAgeDays = 1,
            MaxPasswordAgeDays = 90,
            CommonPasswordBlacklist = new List<string> { "password", "123456", "qwerty" }
        };

        _service = new PasswordPolicyService(
            _dbContext,
            _passwordHasherMock.Object,
            Options.Create(_options),
            _loggerMock.Object);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }

    [Fact]
    public async Task ValidatePassword_WithValidPassword_ReturnsSuccess()
    {
        // Arrange
        var password = "ValidPass123!";
        var email = "user@example.com";

        // Act
        var result = await _service.ValidatePasswordAsync(password, email);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Theory]
    [InlineData("short1!", "Password must be at least 8 characters long")]
    [InlineData("nouppercase123!", "Password must contain at least one uppercase letter")]
    [InlineData("NOLOWERCASE123!", "Password must contain at least one lowercase letter")]
    [InlineData("NoDigitsHere!", "Password must contain at least one digit")]
    [InlineData("NoSpecialChar123", "Password must contain at least one special character")]
    public async Task ValidatePassword_WithInvalidFormat_ReturnsFailure(string password, string expectedError)
    {
        // Arrange
        var email = "user@example.com";

        // Act
        var result = await _service.ValidatePasswordAsync(password, email);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Be(expectedError);
    }

    [Theory]
    [InlineData("password123!", "Password is too common")]
    [InlineData("123456Abc!", "Password is too common")]
    [InlineData("Qwerty123!", "Password is too common")]
    public async Task ValidatePassword_WithCommonPassword_ReturnsFailure(string password, string expectedError)
    {
        // Arrange
        var email = "user@example.com";

        // Act
        var result = await _service.ValidatePasswordAsync(password, email);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Be(expectedError);
    }

    [Fact]
    public async Task ValidatePassword_WithEmailInPassword_ReturnsFailure()
    {
        // Arrange
        var email = "john.doe@example.com";
        var password = "JohnDoe123!";

        // Act
        var result = await _service.ValidatePasswordAsync(password, email);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Be("Password must not contain parts of your email address");
    }

    [Fact]
    public async Task CanChangePasswordAsync_WithRecentChange_ReturnsFalse()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User("test@example.com", "HashedPass", "Test", "User");
        typeof(User).GetProperty("Id")!.SetValue(user, userId);
        typeof(User).GetProperty("LastPasswordChangeAt")!.SetValue(user, DateTime.UtcNow.AddHours(-12));

        await _dbContext.Users.AddAsync(user);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _service.CanChangePasswordAsync(userId);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task CanChangePasswordAsync_AfterMinimumAge_ReturnsTrue()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User("test@example.com", "HashedPass", "Test", "User");
        typeof(User).GetProperty("Id")!.SetValue(user, userId);
        typeof(User).GetProperty("LastPasswordChangeAt")!.SetValue(user, DateTime.UtcNow.AddDays(-2));

        await _dbContext.Users.AddAsync(user);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _service.CanChangePasswordAsync(userId);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsPasswordExpiredAsync_WithExpiredPassword_ReturnsTrue()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User("test@example.com", "HashedPass", "Test", "User");
        typeof(User).GetProperty("Id")!.SetValue(user, userId);
        typeof(User).GetProperty("LastPasswordChangeAt")!.SetValue(user, DateTime.UtcNow.AddDays(-100));

        await _dbContext.Users.AddAsync(user);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _service.IsPasswordExpiredAsync(userId);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task WasPasswordUsedRecentlyAsync_WithReusedPassword_ReturnsTrue()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User("test@example.com", "HashedPass", "Test", "User");
        typeof(User).GetProperty("Id")!.SetValue(user, userId);

        var passwordHistory = new PasswordHistory(userId, "OldHashedPassword");
        
        await _dbContext.Users.AddAsync(user);
        await _dbContext.PasswordHistory.AddAsync(passwordHistory);
        await _dbContext.SaveChangesAsync();

        _passwordHasherMock.Setup(x => x.VerifyPassword("NewPassword123!", "OldHashedPassword"))
            .Returns(true);

        // Act
        var result = await _service.WasPasswordUsedRecentlyAsync(userId, "NewPassword123!");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task RecordPasswordChangeAsync_AddsPasswordHistory()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var hashedPassword = "NewHashedPassword";

        // Act
        await _service.RecordPasswordChangeAsync(userId, hashedPassword);

        // Assert
        var history = await _dbContext.PasswordHistory
            .Where(h => h.UserId == userId)
            .ToListAsync();

        history.Should().ContainSingle();
        history[0].PasswordHash.Should().Be(hashedPassword);
    }

    [Fact]
    public async Task RecordPasswordChangeAsync_EnforcesHistoryLimit()
    {
        // Arrange
        var userId = Guid.NewGuid();

        // Add 5 existing password history records
        for (int i = 0; i < 5; i++)
        {
            var history = new PasswordHistory(userId, $"OldHash{i}");
            typeof(PasswordHistory).GetProperty("CreatedAt")!.SetValue(history, DateTime.UtcNow.AddDays(-i));
            await _dbContext.PasswordHistory.AddAsync(history);
        }
        await _dbContext.SaveChangesAsync();

        // Act
        await _service.RecordPasswordChangeAsync(userId, "NewHash");

        // Assert
        var allHistory = await _dbContext.PasswordHistory
            .Where(h => h.UserId == userId)
            .ToListAsync();

        allHistory.Should().HaveCount(5); // Should keep only 5 most recent
    }

    [Theory]
    [InlineData("a")]
    [InlineData("ab")]
    [InlineData("abc")]
    public async Task ValidatePassword_WithTooShortPassword_ReturnsFailure(string password)
    {
        // Arrange
        var email = "user@example.com";

        // Act
        var result = await _service.ValidatePasswordAsync(password, email);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Be("Password must be at least 8 characters long");
    }

    [Fact]
    public async Task ValidatePassword_WithTooLongPassword_ReturnsFailure()
    {
        // Arrange
        var password = new string('A', 129) + "bc1!"; // 133 characters
        var email = "user@example.com";

        // Act
        var result = await _service.ValidatePasswordAsync(password, email);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Be("Password must not exceed 128 characters");
    }

    [Fact]
    public async Task CleanupOldPasswordHistoryAsync_RemovesOldRecords()
    {
        // Arrange
        var userId = Guid.NewGuid();

        // Add old password history (older than max age)
        var oldHistory = new PasswordHistory(userId, "VeryOldHash");
        typeof(PasswordHistory).GetProperty("CreatedAt")!.SetValue(oldHistory, DateTime.UtcNow.AddDays(-100));
        await _dbContext.PasswordHistory.AddAsync(oldHistory);

        // Add recent password history
        var recentHistory = new PasswordHistory(userId, "RecentHash");
        typeof(PasswordHistory).GetProperty("CreatedAt")!.SetValue(recentHistory, DateTime.UtcNow.AddDays(-10));
        await _dbContext.PasswordHistory.AddAsync(recentHistory);

        await _dbContext.SaveChangesAsync();

        // Act
        await _service.CleanupOldPasswordHistoryAsync();

        // Assert
        var remaining = await _dbContext.PasswordHistory.ToListAsync();
        remaining.Should().ContainSingle();
        remaining[0].PasswordHash.Should().Be("RecentHash");
    }
}
