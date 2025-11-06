using Microsoft.Extensions.Logging;
using WebSearchIndexing.Modules.Identity.Application.Abstractions;
using WebSearchIndexing.Modules.Identity.Application.DTOs;
using WebSearchIndexing.Modules.Identity.Domain.Entities;
using WebSearchIndexing.Modules.Identity.Domain.Repositories;

namespace WebSearchIndexing.Modules.Identity.Application.Services;

internal sealed class InvitationManagerService : IInvitationManagerService
{
    private readonly IUserInvitationRepository _invitationRepository;
    private readonly IUserRepository _userRepository;
    private readonly ITenantRepository _tenantRepository;
    private readonly ILogger<InvitationManagerService> _logger;

    public InvitationManagerService(
        IUserInvitationRepository invitationRepository,
        IUserRepository userRepository,
        ITenantRepository tenantRepository,
        ILogger<InvitationManagerService> logger)
    {
        _invitationRepository = invitationRepository;
        _userRepository = userRepository;
        _tenantRepository = tenantRepository;
        _logger = logger;
    }

    public async Task<OperationResult> SendInvitationAsync(
        string email, 
        Guid invitedByUserId, 
        string role, 
        Guid? tenantId = null, 
        Guid? domainId = null, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(email);
            ArgumentException.ThrowIfNullOrWhiteSpace(role);

            // Validate the inviting user exists
            var invitingUser = await _userRepository.GetByIdAsync(invitedByUserId, cancellationToken);
            if (invitingUser == null)
            {
                _logger.LogWarning("Inviting user {UserId} not found", invitedByUserId);
                return new OperationResult
                {
                    Success = false,
                    Message = "Inviting user not found"
                };
            }

            // Check if tenant exists (if specified)
            if (tenantId.HasValue)
            {
                var tenant = await _tenantRepository.GetByIdAsync(tenantId.Value, cancellationToken);
                if (tenant == null)
                {
                    _logger.LogWarning("Tenant {TenantId} not found for invitation", tenantId.Value);
                    return new OperationResult
                    {
                        Success = false,
                        Message = "Tenant not found"
                    };
                }
            }

            // Check if user with this email already exists
            var existingUser = await _userRepository.GetByEmailAsync(email, cancellationToken);
            if (existingUser != null)
            {
                _logger.LogWarning("User with email {Email} already exists", email);
                return new OperationResult
                {
                    Success = false,
                    Message = "A user with this email address already exists"
                };
            }

            // Check if there's already a pending invitation for this email
            var existingInvitation = await _invitationRepository.GetPendingByEmailAsync(email, cancellationToken);
            if (existingInvitation != null)
            {
                _logger.LogWarning("Pending invitation already exists for email {Email}", email);
                return new OperationResult
                {
                    Success = false,
                    Message = "A pending invitation already exists for this email address"
                };
            }

            // Create the invitation
            var invitation = new UserInvitation(
                email: email,
                invitedByUserId: invitedByUserId,
                role: role,
                tenantId: tenantId,
                domainId: domainId,
                validFor: TimeSpan.FromDays(7)); // 7 days validity

            await _invitationRepository.AddAsync(invitation, cancellationToken);

            _logger.LogInformation("Invitation {InvitationId} sent to {Email} by user {UserId}", 
                invitation.Id, email, invitedByUserId);

            // TODO: Send email notification here
            // await _emailService.SendInvitationEmailAsync(invitation);

            return new OperationResult
            {
                Success = true,
                Message = "Invitation sent successfully"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending invitation to {Email} by user {UserId}", email, invitedByUserId);
            return new OperationResult
            {
                Success = false,
                Message = "An error occurred while sending the invitation"
            };
        }
    }

    public async Task<OperationResult> ResendInvitationAsync(
        Guid invitationId, 
        Guid userId, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            var invitation = await _invitationRepository.GetByIdAsync(invitationId, cancellationToken);
            if (invitation == null)
            {
                _logger.LogWarning("Invitation {InvitationId} not found for resending", invitationId);
                return new OperationResult
                {
                    Success = false,
                    Message = "Invitation not found"
                };
            }

            // Only the person who sent the invitation can resend it
            if (invitation.InvitedByUserId != userId)
            {
                _logger.LogWarning("User {UserId} attempted to resend invitation {InvitationId} they didn't send", 
                    userId, invitationId);
                return new OperationResult
                {
                    Success = false,
                    Message = "You can only resend invitations you sent"
                };
            }

            if (invitation.IsUsed)
            {
                _logger.LogWarning("Cannot resend invitation {InvitationId}: already accepted", invitationId);
                return new OperationResult
                {
                    Success = false,
                    Message = "Cannot resend an invitation that has already been accepted"
                };
            }

            if (invitation.IsRevoked)
            {
                _logger.LogWarning("Cannot resend invitation {InvitationId}: revoked", invitationId);
                return new OperationResult
                {
                    Success = false,
                    Message = "Cannot resend a revoked invitation"
                };
            }

            // TODO: Send email notification here
            // await _emailService.SendInvitationEmailAsync(invitation);

            _logger.LogInformation("Invitation {InvitationId} resent by user {UserId}", invitationId, userId);

            return new OperationResult
            {
                Success = true,
                Message = "Invitation resent successfully"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resending invitation {InvitationId} by user {UserId}", invitationId, userId);
            return new OperationResult
            {
                Success = false,
                Message = "An error occurred while resending the invitation"
            };
        }
    }

    public async Task<InvitationResult> GetInvitationsByTenantAsync(
        Guid tenantId, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            var invitations = await _invitationRepository.GetByTenantAsync(tenantId, cancellationToken);
            var invitationDtos = new List<InvitationDto>();

            var tenant = await _tenantRepository.GetByIdAsync(tenantId, cancellationToken);
            var tenantName = tenant?.Name ?? "Unknown";

            foreach (var invitation in invitations)
            {
                var invitedByUser = await _userRepository.GetByIdAsync(invitation.InvitedByUserId, cancellationToken);
                var invitedByName = invitedByUser?.Email ?? "Unknown";

                invitationDtos.Add(InvitationDto.FromDomain(invitation, tenantName, invitedByName));
            }

            _logger.LogInformation("Retrieved {Count} invitations for tenant {TenantId}", 
                invitationDtos.Count, tenantId);

            return new InvitationResult
            {
                Success = true,
                Message = $"Found {invitationDtos.Count} invitations",
                Invitations = invitationDtos
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting invitations for tenant {TenantId}", tenantId);
            return new InvitationResult
            {
                Success = false,
                Message = "An error occurred while retrieving invitations",
                Invitations = new List<InvitationDto>()
            };
        }
    }

    public async Task<OperationResult> BulkInviteAsync(
        IEnumerable<string> emails, 
        Guid invitedByUserId, 
        string role, 
        Guid? tenantId = null, 
        Guid? domainId = null, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            ArgumentNullException.ThrowIfNull(emails);
            ArgumentException.ThrowIfNullOrWhiteSpace(role);

            var emailList = emails.ToList();
            if (!emailList.Any())
            {
                return new OperationResult
                {
                    Success = false,
                    Message = "No email addresses provided"
                };
            }

            var successCount = 0;
            var failureCount = 0;
            var errors = new List<string>();

            foreach (var email in emailList)
            {
                var result = await SendInvitationAsync(email, invitedByUserId, role, tenantId, domainId, cancellationToken);
                if (result.Success)
                {
                    successCount++;
                }
                else
                {
                    failureCount++;
                    errors.Add($"{email}: {result.Message}");
                }
            }

            var message = $"Sent {successCount} invitations successfully";
            if (failureCount > 0)
            {
                message += $", {failureCount} failed";
            }

            _logger.LogInformation("Bulk invite completed: {SuccessCount} success, {FailureCount} failures", 
                successCount, failureCount);

            return new OperationResult
            {
                Success = successCount > 0,
                Message = message
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in bulk invite by user {UserId}", invitedByUserId);
            return new OperationResult
            {
                Success = false,
                Message = "An error occurred while sending bulk invitations"
            };
        }
    }
}
