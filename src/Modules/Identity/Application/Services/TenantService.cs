using Microsoft.Extensions.Logging;
using WebSearchIndexing.Modules.Identity.Application.Abstractions;
using WebSearchIndexing.Modules.Identity.Domain.Repositories;
using WebSearchIndexing.Modules.Identity.Domain.Entities;
using WebSearchIndexing.Modules.Identity.Domain.Constants;

namespace WebSearchIndexing.Modules.Identity.Application.Services;

public class TenantService : ITenantService
{
    private readonly ITenantRepository _tenantRepository;
    private readonly IUserRepository _userRepository;
    private readonly IUserTenantRepository _userTenantRepository;
    private readonly ILogger<TenantService> _logger;

    public TenantService(
        ITenantRepository tenantRepository, 
        IUserRepository userRepository,
        IUserTenantRepository userTenantRepository,
        ILogger<TenantService> logger)
    {
        _tenantRepository = tenantRepository;
        _userRepository = userRepository;
        _userTenantRepository = userTenantRepository;
        _logger = logger;
    }

    public async Task<GetTenantsResult> GetUserTenantsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Getting tenants for user {UserId}", userId);

            var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
            if (user == null)
            {
                _logger.LogWarning("User {UserId} not found when getting tenants", userId);
                return new GetTenantsResult(false, "User not found");
            }

            // Get user-tenant relationships directly from repository
            var userTenants = await _userTenantRepository.GetByUserAsync(userId, cancellationToken);
            _logger.LogDebug("Found {Count} user-tenant relationships for user {UserId}", userTenants.Count, userId);
            
            // Log each UserTenant for debugging
            foreach (var ut in userTenants)
            {
                _logger.LogDebug("UserTenant: Id={Id}, UserId={UserId}, TenantId={TenantId}, Role={Role}, IsActive={IsActive}", 
                    ut.Id, ut.UserId, ut.TenantId, ut.Role, ut.IsActive);
            }

            var tenants = new List<TenantInfo>();
            foreach (var userTenant in userTenants.Where(ut => ut.IsActive))
            {
                var tenant = await _tenantRepository.GetByIdAsync(userTenant.TenantId, cancellationToken);
                if (tenant != null)
                {
                    tenants.Add(new TenantInfo
                    {
                        Id = tenant.Id,
                        Name = tenant.Name,
                        Plan = tenant.Plan,
                        CreatedAt = tenant.CreatedAt,
                        IsActive = tenant.IsActive,
                        UserRole = userTenant.Role
                    });
                }
            }

            _logger.LogInformation("Retrieved {Count} active tenants for user {UserId}", tenants.Count, userId);
            return new GetTenantsResult(true, "Success", tenants);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving tenants for user {UserId}", userId);
            return new GetTenantsResult(false, "An error occurred while retrieving tenants");
        }
    }

    public async Task<CreateTenantResult> CreateTenantAsync(Guid userId, CreateTenantRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Creating tenant {TenantName} for user {UserId}", request.Name, userId);

            var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
            if (user == null)
            {
                _logger.LogWarning("User {UserId} not found when creating tenant", userId);
                return new CreateTenantResult(false, "User not found");
            }

            _logger.LogDebug("User {UserId} found, current tenant count: {TenantCount}", userId, user.UserTenants.Count);

            // Create tenant first
            var tenant = new Tenant(request.Name, GenerateSlug(request.Name), userId, request.Plan ?? "Free");
            _logger.LogDebug("Creating tenant with ID {TenantId}", tenant.Id);
            
            var createdTenant = await _tenantRepository.AddAsync(tenant, cancellationToken);
            _logger.LogInformation("Tenant {TenantId} created successfully", createdTenant.Id);

            // Create UserTenant relationship directly using the repository
            _logger.LogDebug("Creating UserTenant relationship for user {UserId} and tenant {TenantId}", userId, createdTenant.Id);
            var userTenant = new UserTenant(userId, createdTenant.Id, Roles.Owner);
            var createdUserTenant = await _userTenantRepository.AddAsync(userTenant, cancellationToken);
            
            _logger.LogInformation("UserTenant {UserTenantId} created: User {UserId} is now owner of tenant {TenantId}", 
                createdUserTenant.Id, userId, createdTenant.Id);

            _logger.LogInformation("Successfully created tenant {TenantId} for user {UserId}", createdTenant.Id, userId);
            return new CreateTenantResult(true, "Tenant created successfully", createdTenant.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating tenant {TenantName} for user {UserId}", request.Name, userId);
            return new CreateTenantResult(false, $"An error occurred while creating tenant: {ex.Message}");
        }
    }

    public async Task<GetTenantResult> GetTenantAsync(Guid tenantId, Guid userId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Getting tenant {TenantId} for user {UserId}", tenantId, userId);

            var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
            if (user == null)
            {
                _logger.LogWarning("User {UserId} not found when getting tenant {TenantId}", userId, tenantId);
                return new GetTenantResult(false, "User not found");
            }

            // Check if user has access to this tenant using repository
            var userTenant = await _userTenantRepository.GetByUserAndTenantAsync(userId, tenantId, cancellationToken);
            if (userTenant == null || !userTenant.IsActive)
            {
                _logger.LogWarning("User {UserId} has no access to tenant {TenantId}", userId, tenantId);
                return new GetTenantResult(false, "Access denied to this tenant");
            }

            var tenant = await _tenantRepository.GetByIdAsync(tenantId, cancellationToken);
            if (tenant == null)
            {
                _logger.LogWarning("Tenant {TenantId} not found", tenantId);
                return new GetTenantResult(false, "Tenant not found");
            }

            var tenantInfo = new TenantInfo
            {
                Id = tenant.Id,
                Name = tenant.Name,
                Plan = tenant.Plan,
                CreatedAt = tenant.CreatedAt,
                IsActive = tenant.IsActive,
                UserRole = userTenant.Role
            };

            _logger.LogInformation("Retrieved tenant {TenantId} for user {UserId} with role {Role}", tenantId, userId, userTenant.Role);
            return new GetTenantResult(true, "Success", tenantInfo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving tenant {TenantId} for user {UserId}", tenantId, userId);
            return new GetTenantResult(false, "An error occurred while retrieving tenant");
        }
    }

    public async Task<UpdateTenantResult> UpdateTenantAsync(Guid tenantId, Guid userId, UpdateTenantRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Updating tenant {TenantId} by user {UserId}", tenantId, userId);

            var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
            if (user == null)
            {
                _logger.LogWarning("User {UserId} not found when updating tenant {TenantId}", userId, tenantId);
                return new UpdateTenantResult(false, "User not found");
            }

            // Check if user has admin access to this tenant using repository
            var userTenant = await _userTenantRepository.GetByUserAndTenantAsync(userId, tenantId, cancellationToken);
            if (userTenant == null || !userTenant.IsActive || (userTenant.Role != Roles.Admin && userTenant.Role != Roles.Owner))
            {
                _logger.LogWarning("User {UserId} lacks admin access to tenant {TenantId} (Role: {Role})", userId, tenantId, userTenant?.Role ?? "None");
                return new UpdateTenantResult(false, "Admin access required to update tenant");
            }

            var tenant = await _tenantRepository.GetByIdAsync(tenantId, cancellationToken);
            if (tenant == null)
            {
                _logger.LogWarning("Tenant {TenantId} not found for update", tenantId);
                return new UpdateTenantResult(false, "Tenant not found");
            }

            tenant.UpdateDetails(request.Name, GenerateSlug(request.Name), request.IsActive);
            if (!string.IsNullOrEmpty(request.Plan))
            {
                tenant.ChangePlan(request.Plan);
            }
            await _tenantRepository.UpdateAsync(tenant, cancellationToken);

            _logger.LogInformation("Tenant {TenantId} updated successfully by user {UserId}", tenantId, userId);
            return new UpdateTenantResult(true, "Tenant updated successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating tenant {TenantId} by user {UserId}", tenantId, userId);
            return new UpdateTenantResult(false, "An error occurred while updating tenant");
        }
    }

    public async Task<InviteUserResult> InviteUserToTenantAsync(Guid tenantId, Guid userId, InviteUserRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Inviting user {Email} to tenant {TenantId} by user {UserId}", request.Email, tenantId, userId);

            var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
            if (user == null)
            {
                _logger.LogWarning("User {UserId} not found when inviting to tenant {TenantId}", userId, tenantId);
                return new InviteUserResult(false, "User not found");
            }

            // Check if user has admin access to this tenant using repository
            var userTenant = await _userTenantRepository.GetByUserAndTenantAsync(userId, tenantId, cancellationToken);
            if (userTenant == null || !userTenant.IsActive || (userTenant.Role != Roles.Admin && userTenant.Role != Roles.Owner))
            {
                _logger.LogWarning("User {UserId} lacks admin access to tenant {TenantId} (Role: {Role})", userId, tenantId, userTenant?.Role ?? "None");
                return new InviteUserResult(false, "Admin access required to invite users");
            }

            var tenant = await _tenantRepository.GetByIdAsync(tenantId, cancellationToken);
            if (tenant == null)
            {
                _logger.LogWarning("Tenant {TenantId} not found for invitation", tenantId);
                return new InviteUserResult(false, "Tenant not found");
            }

            var invitedUser = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);
            if (invitedUser == null)
            {
                _logger.LogWarning("User with email {Email} not found for invitation to tenant {TenantId}", request.Email, tenantId);
                return new InviteUserResult(false, "User with this email does not exist");
            }

            // Check if user is already in tenant
            var existingUserTenant = await _userTenantRepository.GetByUserAndTenantAsync(invitedUser.Id, tenantId, cancellationToken);
            if (existingUserTenant != null && existingUserTenant.IsActive)
            {
                _logger.LogWarning("User {UserId} is already in tenant {TenantId}", invitedUser.Id, tenantId);
                return new InviteUserResult(false, "User is already a member of this tenant");
            }

            // Add user to tenant with specified role using repository
            var role = request.Role ?? Roles.Member;
            var newUserTenant = new UserTenant(invitedUser.Id, tenantId, role, userId);
            await _userTenantRepository.AddAsync(newUserTenant, cancellationToken);

            _logger.LogInformation("User {UserId} ({Email}) invited to tenant {TenantId} with role {Role}", invitedUser.Id, request.Email, tenantId, role);
            return new InviteUserResult(true, "User invited successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inviting user {Email} to tenant {TenantId} by user {UserId}", request.Email, tenantId, userId);
            return new InviteUserResult(false, "An error occurred while inviting user");
        }
    }

    private static string GenerateSlug(string name)
    {
        return name.ToLowerInvariant()
            .Replace(" ", "-")
            .Replace("_", "-")
            .Trim('-');
    }
}

// Result models
public record GetTenantsResult(bool Success, string Message, List<TenantInfo>? Tenants = null);
public record CreateTenantResult(bool Success, string Message, Guid? TenantId = null);
public record GetTenantResult(bool Success, string Message, TenantInfo? Tenant = null);
public record UpdateTenantResult(bool Success, string Message);
public record InviteUserResult(bool Success, string Message);

public record CreateTenantRequest(string Name, string? Plan = null);
public record UpdateTenantRequest(string Name, string? Plan = null, bool IsActive = true);
public record InviteUserRequest(string Email, string? Role = null);

public class TenantInfo
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Plan { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public bool IsActive { get; set; }
    public string UserRole { get; set; } = string.Empty;
}
