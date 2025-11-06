using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using WebSearchIndexing.Modules.Identity.Application.Authorization.Policies;
using WebSearchIndexing.Modules.Identity.Domain.Services;

namespace WebSearchIndexing.Modules.Identity.Application.Authorization.Handlers;

public class ResourceAccessHandler : AuthorizationHandler<ResourceAccessRequirement>
{
    private readonly IAuthorizationDomainService _authorizationService;

    public ResourceAccessHandler(IAuthorizationDomainService authorizationService)
    {
        _authorizationService = authorizationService ?? throw new ArgumentNullException(nameof(authorizationService));
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        ResourceAccessRequirement requirement)
    {
        var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
        {
            return;
        }

        // Get resource information from context
        if (context.Resource is ResourceAuthorizationContext resourceContext)
        {
            var hasAccess = await _authorizationService.CanAccessResourceAsync(
                userId,
                resourceContext.ResourceType,
                resourceContext.ResourceId,
                resourceContext.Permission);

            if (hasAccess)
            {
                context.Succeed(requirement);
            }
        }
    }
}
