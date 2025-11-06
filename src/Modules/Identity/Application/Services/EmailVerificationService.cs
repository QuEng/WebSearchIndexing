using Microsoft.Extensions.Configuration;
using WebSearchIndexing.Modules.Identity.Application.Security;
using WebSearchIndexing.Modules.Identity.Domain.Repositories;

namespace WebSearchIndexing.Modules.Identity.Application.Services;

/// <summary>
/// Service for managing email verification
/// </summary>
public interface IEmailVerificationService
{
    /// <summary>
    /// Sends verification email to a user
    /// </summary>
    Task SendVerificationEmailAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Verifies an email using the provided token
    /// </summary>
    Task<bool> VerifyEmailAsync(string token, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a user's email is verified
    /// </summary>
    Task<bool> IsEmailVerifiedAsync(Guid userId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Implementation of email verification service
/// </summary>
public sealed class EmailVerificationService : IEmailVerificationService
{
    private readonly IEmailVerificationRepository _verificationRepository;
    private readonly IUserRepository _userRepository;
    private readonly IIdentityEmailService _emailService;
    private readonly ISecurityLoggingService _securityLogging;
    private readonly IConfiguration _configuration;

    public EmailVerificationService(
        IEmailVerificationRepository verificationRepository,
        IUserRepository userRepository,
        IIdentityEmailService emailService,
        ISecurityLoggingService securityLogging,
        IConfiguration configuration)
    {
        _verificationRepository = verificationRepository ?? throw new ArgumentNullException(nameof(verificationRepository));
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
        _securityLogging = securityLogging ?? throw new ArgumentNullException(nameof(securityLogging));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
    }

    public async Task SendVerificationEmailAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken)
            ?? throw new InvalidOperationException($"User with ID {userId} not found");

        if (user.IsEmailVerified)
        {
            throw new InvalidOperationException("Email is already verified");
        }

        // Create verification token
        var token = await _verificationRepository.CreateTokenAsync(userId, cancellationToken);

        // Build verification link
        var baseUrl = _configuration["Identity:BaseUrl"] ?? "https://localhost:7001";
        var verificationLink = $"{baseUrl}/verify-email?token={token.Token}";

        // Send verification email
        await _emailService.SendEmailVerificationAsync(
            user.Email,
            user.FirstName,
            verificationLink,
            cancellationToken);

        _securityLogging.LogSecurityEvent(new SecurityEvent
        {
            EventType = SecurityEventType.EmailVerificationSent,
            UserId = userId.ToString(),
            Email = user.Email,
            AdditionalInfo = "Email verification sent"
        });
    }

    public async Task<bool> VerifyEmailAsync(string token, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(token);

        var verificationToken = await _verificationRepository.GetByTokenAsync(token, cancellationToken);
        
        if (verificationToken == null)
        {
            _securityLogging.LogSecurityEvent(new SecurityEvent
            {
                EventType = SecurityEventType.InvalidTokenAttempt,
                AdditionalInfo = $"Token not found in database: {token}"
            });
            return false;
        }

        if (!verificationToken.IsValid())
        {
            _securityLogging.LogSecurityEvent(new SecurityEvent
            {
                EventType = SecurityEventType.InvalidTokenAttempt,
                AdditionalInfo = $"Token validation failed - IsUsed: {verificationToken.IsUsed}, ExpiresAt: {verificationToken.ExpiresAt}, Now: {DateTime.UtcNow}, Token: {token}"
            });
            return false;
        }

        // Mark token as used
        await _verificationRepository.MarkAsUsedAsync(verificationToken.Id, cancellationToken);

        // Use the user from the verification token (already loaded with navigation property)
        var user = verificationToken.User 
            ?? throw new InvalidOperationException($"User with ID {verificationToken.UserId} not found in verification token");

        user.MarkEmailAsVerified();
        await _userRepository.UpdateAsync(user, cancellationToken);

        _securityLogging.LogSecurityEvent(new SecurityEvent
        {
            EventType = SecurityEventType.EmailVerified,
            UserId = user.Id.ToString(),
            Email = user.Email,
            AdditionalInfo = "Email verified successfully"
        });

        return true;
    }

    public async Task<bool> IsEmailVerifiedAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        return user?.IsEmailVerified ?? false;
    }
}
