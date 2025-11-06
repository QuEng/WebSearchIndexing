using WebSearchIndexing.Modules.Identity.Application.Abstractions;
using WebSearchIndexing.Modules.Identity.Domain.Repositories;
using WebSearchIndexing.Modules.Identity.Domain.Entities;

namespace WebSearchIndexing.Modules.Identity.Application.Services;

public class TenantService : ITenantService
{
    private readonly ITenantRepository _tenantRepository;
    private readonly IUserRepository _userRepository;

    public TenantService(ITenantRepository tenantRepository, IUserRepository userRepository)
    {
        _tenantRepository = tenantRepository;
        _userRepository = userRepository;
    }

    public async Task<GetTenantsResult> GetUserTenantsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
            if (user == null)
            {
                return new GetTenantsResult(false, "User not found");
            }

            var tenants = new List<TenantInfo>();
            foreach (var userTenant in user.UserTenants.Where(ut => ut.IsActive))
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

            return new GetTenantsResult(true, "Success", tenants);
        }
        catch (Exception)
        {
            return new GetTenantsResult(false, "An error occurred while retrieving tenants");
        }
    }

    public async Task<CreateTenantResult> CreateTenantAsync(Guid userId, CreateTenantRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
            if (user == null)
            {
                return new CreateTenantResult(false, "User not found");
            }

            // Create tenant
            var tenant = new Tenant(request.Name, GenerateSlug(request.Name), userId, request.Plan ?? "Free");
            await _tenantRepository.AddAsync(tenant, cancellationToken);

            // Add user as admin of the tenant
            user.AddTenantRole(tenant.Id, "Admin");
            await _userRepository.UpdateAsync(user, cancellationToken);

            return new CreateTenantResult(true, "Tenant created successfully", tenant.Id);
        }
        catch (Exception)
        {
            return new CreateTenantResult(false, "An error occurred while creating tenant");
        }
    }

    public async Task<GetTenantResult> GetTenantAsync(Guid tenantId, Guid userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
            if (user == null)
            {
                return new GetTenantResult(false, "User not found");
            }

            // Check if user has access to this tenant
            var userTenant = user.UserTenants.FirstOrDefault(ut => ut.TenantId == tenantId && ut.IsActive);
            if (userTenant == null)
            {
                return new GetTenantResult(false, "Access denied to this tenant");
            }

            var tenant = await _tenantRepository.GetByIdAsync(tenantId, cancellationToken);
            if (tenant == null)
            {
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

            return new GetTenantResult(true, "Success", tenantInfo);
        }
        catch (Exception)
        {
            return new GetTenantResult(false, "An error occurred while retrieving tenant");
        }
    }

    public async Task<UpdateTenantResult> UpdateTenantAsync(Guid tenantId, Guid userId, UpdateTenantRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
            if (user == null)
            {
                return new UpdateTenantResult(false, "User not found");
            }

            // Check if user has admin access to this tenant
            var userTenant = user.UserTenants.FirstOrDefault(ut => ut.TenantId == tenantId && ut.IsActive);
            if (userTenant == null || userTenant.Role != "Admin")
            {
                return new UpdateTenantResult(false, "Admin access required to update tenant");
            }

            var tenant = await _tenantRepository.GetByIdAsync(tenantId, cancellationToken);
            if (tenant == null)
            {
                return new UpdateTenantResult(false, "Tenant not found");
            }

            tenant.UpdateDetails(request.Name, GenerateSlug(request.Name));
            if (!string.IsNullOrEmpty(request.Plan))
            {
                tenant.ChangePlan(request.Plan);
            }
            await _tenantRepository.UpdateAsync(tenant, cancellationToken);

            return new UpdateTenantResult(true, "Tenant updated successfully");
        }
        catch (Exception)
        {
            return new UpdateTenantResult(false, "An error occurred while updating tenant");
        }
    }

    public async Task<InviteUserResult> InviteUserToTenantAsync(Guid tenantId, Guid userId, InviteUserRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
            if (user == null)
            {
                return new InviteUserResult(false, "User not found");
            }

            // Check if user has admin access to this tenant
            var userTenant = user.UserTenants.FirstOrDefault(ut => ut.TenantId == tenantId && ut.IsActive);
            if (userTenant == null || userTenant.Role != "Admin")
            {
                return new InviteUserResult(false, "Admin access required to invite users");
            }

            var tenant = await _tenantRepository.GetByIdAsync(tenantId, cancellationToken);
            if (tenant == null)
            {
                return new InviteUserResult(false, "Tenant not found");
            }

            var invitedUser = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);
            if (invitedUser == null)
            {
                return new InviteUserResult(false, "User with this email does not exist");
            }

            // Add user to tenant with specified role
            var role = request.Role ?? "User";
            invitedUser.AddTenantRole(tenantId, role);
            await _userRepository.UpdateAsync(invitedUser, cancellationToken);

            return new InviteUserResult(true, "User invited successfully");
        }
        catch (Exception)
        {
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
public record UpdateTenantRequest(string Name, string? Plan = null);
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
