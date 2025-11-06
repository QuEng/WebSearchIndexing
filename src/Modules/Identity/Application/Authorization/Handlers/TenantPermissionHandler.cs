using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using WebSearchIndexing.Modules.Identity.Application.Authorization.Policies;
using WebSearchIndexing.Modules.Identity.Domain.Services;

namespace WebSearchIndexing.Modules.Identity.Application.Authorization.Handlers;

public class TenantPermissionHandler : AuthorizationHandler<TenantPermissionRequirement>
{
    private readonly IAuthorizationDomainService _authorizationService;

    public TenantPermissionHandler(IAuthorizationDomainService authorizationService)
    {
        _authorizationService = authorizationService ?? throw new ArgumentNullException(nameof(authorizationService));
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        TenantPermissionRequirement requirement)
    {
        var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier);
        var tenantIdClaim = context.User.FindFirst("tenant_id");

        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId) ||
            tenantIdClaim == null || !Guid.TryParse(tenantIdClaim.Value, out var tenantId))
        {
            return;
        }

        var hasPermission = await _authorizationService.HasTenantPermissionAsync(
            userId, tenantId, requirement.Permission);

        if (hasPermission)
        {
            context.Succeed(requirement);
        }
    }
}
