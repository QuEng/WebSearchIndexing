using Microsoft.Extensions.Logging;
using WebSearchIndexing.Modules.Identity.Application.Abstractions;
using WebSearchIndexing.Modules.Identity.Application.DTOs;
using WebSearchIndexing.Modules.Identity.Domain.Repositories;

namespace WebSearchIndexing.Modules.Identity.Application.Services;

internal sealed class InvitationService : IInvitationService
{
    private readonly IUserInvitationRepository _invitationRepository;
    private readonly IUserRepository _userRepository;
    private readonly ITenantRepository _tenantRepository;
    private readonly ILogger<InvitationService> _logger;

    public InvitationService(
        IUserInvitationRepository invitationRepository,
        IUserRepository userRepository,
        ITenantRepository tenantRepository,
        ILogger<InvitationService> logger)
    {
        _invitationRepository = invitationRepository;
        _userRepository = userRepository;
        _tenantRepository = tenantRepository;
        _logger = logger;
    }

    public async Task<InvitationResult> GetIncomingInvitationsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
            if (user == null)
            {
                _logger.LogWarning("User with ID {UserId} not found when getting incoming invitations", userId);
                return new InvitationResult
                {
                    Success = false,
                    Message = "User not found",
                    Invitations = new List<InvitationDto>()
                };
            }

            // Get all pending invitations for user's email
            var userEmail = user.Email;
            var allInvitations = await _invitationRepository.GetPendingInvitationsAsync(cancellationToken);
            var userInvitations = allInvitations.Where(inv => inv.Email.Equals(userEmail, StringComparison.OrdinalIgnoreCase)).ToList();

            var invitationDtos = new List<InvitationDto>();

            foreach (var invitation in userInvitations)
            {
                var tenantName = "Unknown";
                var invitedByName = "Unknown";

                if (invitation.TenantId.HasValue)
                {
                    var tenant = await _tenantRepository.GetByIdAsync(invitation.TenantId.Value, cancellationToken);
                    tenantName = tenant?.Name ?? "Unknown";
                }

                var invitedByUser = await _userRepository.GetByIdAsync(invitation.InvitedByUserId, cancellationToken);
                invitedByName = invitedByUser?.Email ?? "Unknown";

                invitationDtos.Add(InvitationDto.FromDomain(invitation, tenantName, invitedByName));
            }

            _logger.LogInformation("Retrieved {Count} incoming invitations for user {UserId}", invitationDtos.Count, userId);

            return new InvitationResult
            {
                Success = true,
                Message = $"Found {invitationDtos.Count} incoming invitations",
                Invitations = invitationDtos
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting incoming invitations for user {UserId}", userId);
            return new InvitationResult
            {
                Success = false,
                Message = "An error occurred while retrieving incoming invitations",
                Invitations = new List<InvitationDto>()
            };
        }
    }

    public async Task<InvitationResult> GetOutgoingInvitationsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
            if (user == null)
            {
                _logger.LogWarning("User with ID {UserId} not found when getting outgoing invitations", userId);
                return new InvitationResult
                {
                    Success = false,
                    Message = "User not found",
                    Invitations = new List<InvitationDto>()
                };
            }

            // Get all invitations sent by this user
            var allInvitations = await _invitationRepository.GetPendingInvitationsAsync(cancellationToken);
            var userSentInvitations = allInvitations.Where(inv => inv.InvitedByUserId == userId).ToList();

            var invitationDtos = new List<InvitationDto>();

            foreach (var invitation in userSentInvitations)
            {
                var tenantName = "Unknown";

                if (invitation.TenantId.HasValue)
                {
                    var tenant = await _tenantRepository.GetByIdAsync(invitation.TenantId.Value, cancellationToken);
                    tenantName = tenant?.Name ?? "Unknown";
                }

                invitationDtos.Add(InvitationDto.FromDomain(invitation, tenantName, user.Email));
            }

            _logger.LogInformation("Retrieved {Count} outgoing invitations for user {UserId}", invitationDtos.Count, userId);

            return new InvitationResult
            {
                Success = true,
                Message = $"Found {invitationDtos.Count} outgoing invitations",
                Invitations = invitationDtos
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting outgoing invitations for user {UserId}", userId);
            return new InvitationResult
            {
                Success = false,
                Message = "An error occurred while retrieving outgoing invitations",
                Invitations = new List<InvitationDto>()
            };
        }
    }

    public async Task<InvitationDetailsResult> GetInvitationByTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        try
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(token);

            var invitation = await _invitationRepository.GetByTokenAsync(token, cancellationToken);
            if (invitation == null)
            {
                _logger.LogWarning("Invitation with token {Token} not found", token);
                return new InvitationDetailsResult
                {
                    Success = false,
                    Message = "Invitation not found",
                    Invitation = null
                };
            }

            var tenantName = "Unknown";
            var invitedByName = "Unknown";

            if (invitation.TenantId.HasValue)
            {
                var tenant = await _tenantRepository.GetByIdAsync(invitation.TenantId.Value, cancellationToken);
                tenantName = tenant?.Name ?? "Unknown";
            }

            var invitedByUser = await _userRepository.GetByIdAsync(invitation.InvitedByUserId, cancellationToken);
            invitedByName = invitedByUser?.Email ?? "Unknown";

            var invitationDto = InvitationDto.FromDomain(invitation, tenantName, invitedByName);

            _logger.LogInformation("Retrieved invitation {InvitationId} by token", invitation.Id);

            return new InvitationDetailsResult
            {
                Success = true,
                Message = "Invitation found",
                Invitation = invitationDto
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting invitation by token {Token}", token);
            return new InvitationDetailsResult
            {
                Success = false,
                Message = "An error occurred while retrieving invitation",
                Invitation = null
            };
        }
    }

    public async Task<OperationResult> AcceptInvitationAsync(Guid invitationId, Guid userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var invitation = await _invitationRepository.GetByIdAsync(invitationId, cancellationToken);
            if (invitation == null)
            {
                _logger.LogWarning("Invitation {InvitationId} not found for acceptance", invitationId);
                return new OperationResult
                {
                    Success = false,
                    Message = "Invitation not found"
                };
            }

            if (!invitation.IsValid())
            {
                var reason = invitation.IsUsed ? "already used" : invitation.IsRevoked ? "revoked" : "expired";
                _logger.LogWarning("Cannot accept invitation {InvitationId}: {Reason}", invitationId, reason);
                return new OperationResult
                {
                    Success = false,
                    Message = $"Invitation cannot be accepted: {reason}"
                };
            }

            var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
            if (user == null)
            {
                _logger.LogWarning("User {UserId} not found when accepting invitation {InvitationId}", userId, invitationId);
                return new OperationResult
                {
                    Success = false,
                    Message = "User not found"
                };
            }

            // Verify that the invitation email matches the user's email
            if (!invitation.Email.Equals(user.Email, StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning("Email mismatch when user {UserId} tried to accept invitation {InvitationId}", userId, invitationId);
                return new OperationResult
                {
                    Success = false,
                    Message = "This invitation is not for your email address"
                };
            }

            invitation.Accept(userId);
            await _invitationRepository.UpdateAsync(invitation, cancellationToken);

            _logger.LogInformation("User {UserId} accepted invitation {InvitationId}", userId, invitationId);

            return new OperationResult
            {
                Success = true,
                Message = "Invitation accepted successfully"
            };
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Cannot accept invitation {InvitationId}: {Message}", invitationId, ex.Message);
            return new OperationResult
            {
                Success = false,
                Message = ex.Message
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error accepting invitation {InvitationId} for user {UserId}", invitationId, userId);
            return new OperationResult
            {
                Success = false,
                Message = "An error occurred while accepting the invitation"
            };
        }
    }

    public async Task<OperationResult> DeclineInvitationAsync(Guid invitationId, Guid userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var invitation = await _invitationRepository.GetByIdAsync(invitationId, cancellationToken);
            if (invitation == null)
            {
                _logger.LogWarning("Invitation {InvitationId} not found for declining", invitationId);
                return new OperationResult
                {
                    Success = false,
                    Message = "Invitation not found"
                };
            }

            var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
            if (user == null)
            {
                _logger.LogWarning("User {UserId} not found when declining invitation {InvitationId}", userId, invitationId);
                return new OperationResult
                {
                    Success = false,
                    Message = "User not found"
                };
            }

            // Verify that the invitation email matches the user's email
            if (!invitation.Email.Equals(user.Email, StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning("Email mismatch when user {UserId} tried to decline invitation {InvitationId}", userId, invitationId);
                return new OperationResult
                {
                    Success = false,
                    Message = "This invitation is not for your email address"
                };
            }

            if (invitation.IsUsed)
            {
                _logger.LogWarning("Cannot decline invitation {InvitationId}: already accepted", invitationId);
                return new OperationResult
                {
                    Success = false,
                    Message = "Invitation has already been accepted"
                };
            }

            // For declining, we revoke the invitation
            invitation.Revoke();
            await _invitationRepository.UpdateAsync(invitation, cancellationToken);

            _logger.LogInformation("User {UserId} declined invitation {InvitationId}", userId, invitationId);

            return new OperationResult
            {
                Success = true,
                Message = "Invitation declined successfully"
            };
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Cannot decline invitation {InvitationId}: {Message}", invitationId, ex.Message);
            return new OperationResult
            {
                Success = false,
                Message = ex.Message
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error declining invitation {InvitationId} for user {UserId}", invitationId, userId);
            return new OperationResult
            {
                Success = false,
                Message = "An error occurred while declining the invitation"
            };
        }
    }

    public async Task<OperationResult> RevokeInvitationAsync(Guid invitationId, Guid userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var invitation = await _invitationRepository.GetByIdAsync(invitationId, cancellationToken);
            if (invitation == null)
            {
                _logger.LogWarning("Invitation {InvitationId} not found for revoking", invitationId);
                return new OperationResult
                {
                    Success = false,
                    Message = "Invitation not found"
                };
            }

            // Only the person who sent the invitation can revoke it
            if (invitation.InvitedByUserId != userId)
            {
                _logger.LogWarning("User {UserId} attempted to revoke invitation {InvitationId} they didn't send", userId, invitationId);
                return new OperationResult
                {
                    Success = false,
                    Message = "You can only revoke invitations you sent"
                };
            }

            if (invitation.IsUsed)
            {
                _logger.LogWarning("Cannot revoke invitation {InvitationId}: already accepted", invitationId);
                return new OperationResult
                {
                    Success = false,
                    Message = "Cannot revoke an invitation that has already been accepted"
                };
            }

            if (invitation.IsRevoked)
            {
                _logger.LogWarning("Invitation {InvitationId} is already revoked", invitationId);
                return new OperationResult
                {
                    Success = false,
                    Message = "Invitation is already revoked"
                };
            }

            invitation.Revoke();
            await _invitationRepository.UpdateAsync(invitation, cancellationToken);

            _logger.LogInformation("User {UserId} revoked invitation {InvitationId}", userId, invitationId);

            return new OperationResult
            {
                Success = true,
                Message = "Invitation revoked successfully"
            };
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Cannot revoke invitation {InvitationId}: {Message}", invitationId, ex.Message);
            return new OperationResult
            {
                Success = false,
                Message = ex.Message
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error revoking invitation {InvitationId} for user {UserId}", invitationId, userId);
            return new OperationResult
            {
                Success = false,
                Message = "An error occurred while revoking the invitation"
            };
        }
    }
}
