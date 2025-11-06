using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using WebSearchIndexing.Modules.Identity.Application.Authorization.Policies;

namespace WebSearchIndexing.Modules.Identity.Application.Authorization.Handlers;

public class TenantRoleHandler : AuthorizationHandler<TenantRoleRequirement>
{
    public TenantRoleHandler()
    {
    }

    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        TenantRoleRequirement requirement)
    {
        var tenantIdClaim = context.User.FindFirst("tenant_id");
        if (tenantIdClaim != null && context.User.IsInRole(requirement.Role))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
