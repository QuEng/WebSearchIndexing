using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using WebSearchIndexing.Modules.Identity.Application.Authorization.Policies;
using WebSearchIndexing.Modules.Identity.Domain.Services;

namespace WebSearchIndexing.Modules.Identity.Application.Authorization.Handlers;

public class GlobalPermissionHandler : AuthorizationHandler<GlobalPermissionRequirement>
{
    private readonly IAuthorizationDomainService _authorizationService;

    public GlobalPermissionHandler(IAuthorizationDomainService authorizationService)
    {
        _authorizationService = authorizationService ?? throw new ArgumentNullException(nameof(authorizationService));
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        GlobalPermissionRequirement requirement)
    {
        var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
        {
            return;
        }

        var hasPermission = await _authorizationService.HasGlobalPermissionAsync(
            userId, requirement.Permission);

        if (hasPermission)
        {
            context.Succeed(requirement);
        }
    }
}
