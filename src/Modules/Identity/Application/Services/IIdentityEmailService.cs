namespace WebSearchIndexing.Modules.Identity.Application.Services;

/// <summary>
/// Service for sending emails related to identity operations
/// </summary>
public interface IIdentityEmailService
{
    /// <summary>
    /// Sends a user invitation email
    /// </summary>
    Task SendUserInvitationAsync(
        string recipientEmail,
        string recipientName,
        string inviterName,
        string organizationName,
        string invitationLink,
        string role,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a welcome email to newly registered users
    /// </summary>
    Task SendWelcomeEmailAsync(
        string recipientEmail,
        string recipientName,
        string organizationName,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends password reset email
    /// </summary>
    Task SendPasswordResetEmailAsync(
        string recipientEmail,
        string recipientName,
        string resetLink,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends email verification email
    /// </summary>
    Task SendEmailVerificationAsync(
        string recipientEmail,
        string recipientName,
        string verificationLink,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends security alert email
    /// </summary>
    Task SendSecurityAlertAsync(
        string recipientEmail,
        string recipientName,
        string alertType,
        string alertDetails,
        DateTime occurredAt,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends role assignment notification
    /// </summary>
    Task SendRoleAssignmentNotificationAsync(
        string recipientEmail,
        string recipientName,
        string role,
        string organizationName,
        string assignedBy,
        CancellationToken cancellationToken = default);
}
