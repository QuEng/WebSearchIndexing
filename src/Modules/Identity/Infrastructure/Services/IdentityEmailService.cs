using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RestSharp;
using WebSearchIndexing.Modules.Identity.Application.Services;

namespace WebSearchIndexing.Modules.Identity.Infrastructure.Services;

/// <summary>
/// Email service implementation using MailerSend for Identity module
/// </summary>
public class IdentityEmailService : IIdentityEmailService
{
    private readonly IConfiguration _configuration;
    private readonly RestClient _mailerSendClient;
    private readonly string _mailerSendToken;
    private readonly string _fromEmail;
    private readonly string _fromName;
    private readonly ILogger<IdentityEmailService> _logger;

    public IdentityEmailService(
        IConfiguration configuration,
        ILogger<IdentityEmailService> logger)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _mailerSendClient = new RestClient("https://api.mailersend.com/v1");
        
        _mailerSendToken = _configuration["MailerSend:Token"] ?? string.Empty;
        if (string.IsNullOrWhiteSpace(_mailerSendToken))
        {
            _logger.LogError("MailerSend:Token is not configured in appsettings or user secrets");
            throw new InvalidOperationException("MailerSend token is not configured. Please set 'MailerSend:Token' in configuration.");
        }

        _fromEmail = _configuration["MailerSend:FromEmail"] ?? string.Empty;
        if (string.IsNullOrWhiteSpace(_fromEmail))
        {
            _logger.LogError("MailerSend:FromEmail is not configured in appsettings or user secrets");
            throw new InvalidOperationException("MailerSend from email is not configured. Please set 'MailerSend:FromEmail' in configuration.");
        }

        _fromName = _configuration["MailerSend:FromName"] ?? "WebSearchIndexing";

        _logger.LogInformation(
            "IdentityEmailService initialized. FromEmail: {FromEmail}, FromName: {FromName}, Token length: {TokenLength}",
            _fromEmail, _fromName, _mailerSendToken.Length);
    }

    public async Task SendUserInvitationAsync(
        string recipientEmail,
        string recipientName,
        string inviterName,
        string organizationName,
        string invitationLink,
        string role,
        CancellationToken cancellationToken = default)
    {
        var subject = $"You've been invited to join {organizationName}";
        var htmlBody = $@"
            <html>
            <body>
                <h2>You're invited to join {organizationName}</h2>
                <p>Hello {recipientName},</p>
                <p>{inviterName} has invited you to join <strong>{organizationName}</strong> as a <strong>{role}</strong>.</p>
                <p>Click the link below to accept the invitation:</p>
                <p><a href=""{invitationLink}"" style=""background-color: #007bff; color: white; padding: 10px 20px; text-decoration: none; border-radius: 5px;"">Accept Invitation</a></p>
                <p>This invitation will expire in 7 days.</p>
                <p>If you didn't expect this invitation, you can safely ignore this email.</p>
                <hr>
                <p><small>WebSearchIndexing Team</small></p>
            </body>
            </html>";

        await SendEmailAsync(recipientEmail, recipientName, subject, htmlBody, cancellationToken);
    }

    public async Task SendWelcomeEmailAsync(
        string recipientEmail,
        string recipientName,
        string organizationName,
        CancellationToken cancellationToken = default)
    {
        var subject = $"Welcome to {organizationName}!";
        var htmlBody = $@"
            <html>
            <body>
                <h2>Welcome to {organizationName}!</h2>
                <p>Hello {recipientName},</p>
                <p>Welcome to WebSearchIndexing! Your account has been successfully created.</p>
                <p>You can now start managing your search console indexing operations.</p>
                <p>If you have any questions, feel free to contact our support team.</p>
                <hr>
                <p><small>WebSearchIndexing Team</small></p>
            </body>
            </html>";

        await SendEmailAsync(recipientEmail, recipientName, subject, htmlBody, cancellationToken);
    }

    public async Task SendPasswordResetEmailAsync(
        string recipientEmail,
        string recipientName,
        string resetLink,
        CancellationToken cancellationToken = default)
    {
        var subject = "Reset your password";
        var htmlBody = $@"
            <html>
            <body>
                <h2>Reset your password</h2>
                <p>Hello {recipientName},</p>
                <p>We received a request to reset your password for your WebSearchIndexing account.</p>
                <p>Click the link below to reset your password:</p>
                <p><a href=""{resetLink}"" style=""background-color: #dc3545; color: white; padding: 10px 20px; text-decoration: none; border-radius: 5px;"">Reset Password</a></p>
                <p>This link will expire in 1 hour.</p>
                <p>If you didn't request a password reset, you can safely ignore this email.</p>
                <hr>
                <p><small>WebSearchIndexing Team</small></p>
            </body>
            </html>";

        await SendEmailAsync(recipientEmail, recipientName, subject, htmlBody, cancellationToken);
    }

    public async Task SendEmailVerificationAsync(
        string recipientEmail,
        string recipientName,
        string verificationLink,
        CancellationToken cancellationToken = default)
    {
        var subject = "Verify your email address";
        var htmlBody = $@"
            <html>
            <body>
                <h2>Verify your email address</h2>
                <p>Hello {recipientName},</p>
                <p>Thank you for registering with WebSearchIndexing!</p>
                <p>Please verify your email address by clicking the link below:</p>
                <p><a href=""{verificationLink}"" style=""background-color: #28a745; color: white; padding: 10px 20px; text-decoration: none; border-radius: 5px;"">Verify Email</a></p>
                <p>This link will expire in 24 hours.</p>
                <p>If you didn't create an account, you can safely ignore this email.</p>
                <hr>
                <p><small>WebSearchIndexing Team</small></p>
            </body>
            </html>";

        await SendEmailAsync(recipientEmail, recipientName, subject, htmlBody, cancellationToken);
    }

    public async Task SendSecurityAlertAsync(
        string recipientEmail,
        string recipientName,
        string alertType,
        string alertDetails,
        DateTime occurredAt,
        CancellationToken cancellationToken = default)
    {
        var subject = $"Security Alert: {alertType}";
        var htmlBody = $@"
            <html>
            <body>
                <h2>Security Alert</h2>
                <p>Hello {recipientName},</p>
                <p>We detected unusual activity on your WebSearchIndexing account:</p>
                <p><strong>Alert Type:</strong> {alertType}</p>
                <p><strong>Details:</strong> {alertDetails}</p>
                <p><strong>Occurred At:</strong> {occurredAt:yyyy-MM-dd HH:mm:ss} UTC</p>
                <p>If this was you, no action is needed. If you didn't authorize this activity, please change your password immediately.</p>
                <hr>
                <p><small>WebSearchIndexing Security Team</small></p>
            </body>
            </html>";

        await SendEmailAsync(recipientEmail, recipientName, subject, htmlBody, cancellationToken);
    }

    public async Task SendRoleAssignmentNotificationAsync(
        string recipientEmail,
        string recipientName,
        string role,
        string organizationName,
        string assignedBy,
        CancellationToken cancellationToken = default)
    {
        var subject = $"Your role has been updated in {organizationName}";
        var htmlBody = $@"
            <html>
            <body>
                <h2>Role Assignment Update</h2>
                <p>Hello {recipientName},</p>
                <p>Your role in <strong>{organizationName}</strong> has been updated.</p>
                <p><strong>New Role:</strong> {role}</p>
                <p><strong>Assigned By:</strong> {assignedBy}</p>
                <p>Your new permissions are now active and you can access the corresponding features.</p>
                <hr>
                <p><small>WebSearchIndexing Team</small></p>
            </body>
            </html>";

        await SendEmailAsync(recipientEmail, recipientName, subject, htmlBody, cancellationToken);
    }

    private async Task SendEmailAsync(
        string recipientEmail,
        string recipientName,
        string subject,
        string htmlBody,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation(
                "Attempting to send email to {Email} from {FromEmail} with subject: {Subject}",
                recipientEmail, _fromEmail, subject);

            var emailRequest = new EmailRequestWithBody
            {
                From = new EmailRequestAddress { Email = _fromEmail, Name = _fromName },
                To = [new() { Email = recipientEmail, Name = recipientName }],
                Subject = subject,
                HtmlBody = htmlBody
            };

            var response = await ExecuteMailRequestAsync(emailRequest);
            
            if (!response.IsSuccessful)
            {
                _logger.LogError(
                    "Failed to send email to {Email}. Status: {StatusCode}, Error: {ErrorMessage}, Content: {Content}",
                    recipientEmail, 
                    response.StatusCode, 
                    response.ErrorMessage,
                    response.Content);
                
                throw new InvalidOperationException(
                    $"Failed to send email: Status={response.StatusCode}, Error={response.ErrorMessage}, Content={response.Content}");
            }

            _logger.LogInformation(
                "Email sent successfully to {Email}. MessageId: {MessageId}", 
                recipientEmail, 
                response.Content);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex, 
                "Error sending email to {Email}. FromEmail: {FromEmail}, Token configured: {TokenConfigured}",
                recipientEmail, 
                _fromEmail,
                !string.IsNullOrEmpty(_mailerSendToken));
            throw;
        }
    }

    private async Task<RestResponse> ExecuteMailRequestAsync<T>(T input)
    {
        var request = new RestRequest("email", Method.Post);
        request.AddHeader("Authorization", $"Bearer {_mailerSendToken}");
        request.AddHeader("Content-Type", "application/json");
        
        var requestBody = JsonConvert.SerializeObject(input);
        
        _logger.LogDebug(
            "Sending email request to MailerSend. URL: {Url}, Body length: {BodyLength}", 
            _mailerSendClient.Options.BaseUrl, 
            requestBody.Length);
        
        request.AddParameter("application/json", requestBody, ParameterType.RequestBody);
        
        var response = await _mailerSendClient.ExecuteAsync(request);
        
        _logger.LogDebug(
            "Received response from MailerSend. Status: {StatusCode}, Success: {IsSuccessful}", 
            response.StatusCode, 
            response.IsSuccessful);
        
        return response;
    }
}
