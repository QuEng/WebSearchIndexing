using WebSearchIndexing.Modules.Identity.Application.Services;

namespace WebSearchIndexing.Modules.Identity.Application.Abstractions;

public interface ITenantService
{
    Task<GetTenantsResult> GetUserTenantsAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<CreateTenantResult> CreateTenantAsync(Guid userId, CreateTenantRequest request, CancellationToken cancellationToken = default);
    Task<GetTenantResult> GetTenantAsync(Guid tenantId, Guid userId, CancellationToken cancellationToken = default);
    Task<UpdateTenantResult> UpdateTenantAsync(Guid tenantId, Guid userId, UpdateTenantRequest request, CancellationToken cancellationToken = default);
    Task<InviteUserResult> InviteUserToTenantAsync(Guid tenantId, Guid userId, InviteUserRequest request, CancellationToken cancellationToken = default);
}
