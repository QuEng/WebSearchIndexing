using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using WebSearchIndexing.Modules.Identity.Application.Authorization.Policies;
using WebSearchIndexing.Modules.Identity.Domain.Services;

namespace WebSearchIndexing.Modules.Identity.Application.Authorization.Handlers;

public class DomainPermissionHandler : AuthorizationHandler<DomainPermissionRequirement>
{
    private readonly IAuthorizationDomainService _authorizationService;

    public DomainPermissionHandler(IAuthorizationDomainService authorizationService)
    {
        _authorizationService = authorizationService ?? throw new ArgumentNullException(nameof(authorizationService));
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        DomainPermissionRequirement requirement)
    {
        var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier);
        var domainIdClaim = context.User.FindFirst("domain_id");

        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId) ||
            domainIdClaim == null || !Guid.TryParse(domainIdClaim.Value, out var domainId))
        {
            return;
        }

        var hasPermission = await _authorizationService.HasDomainPermissionAsync(
            userId, domainId, requirement.Permission);

        if (hasPermission)
        {
            context.Succeed(requirement);
        }
    }
}
