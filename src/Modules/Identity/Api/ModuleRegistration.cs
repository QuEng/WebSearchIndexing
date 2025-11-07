using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WebSearchIndexing.Modules.Identity.Api.Endpoints;
using WebSearchIndexing.Modules.Identity.Application;
using WebSearchIndexing.Modules.Identity.Infrastructure;

namespace WebSearchIndexing.Modules.Identity.Api;

public static class IdentityModule
{
    public static IServiceCollection AddIdentityModule(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);
        
        services.AddIdentityApplicationModule();
        services.AddIdentityInfrastructure(configuration);
        
        return services;
    }

    public static IEndpointRouteBuilder MapIdentityModuleEndpoints(this IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);

        var identityGroup = endpoints.MapGroup("/api/v1/identity");
        
        identityGroup.MapAuthEndpoints();
        identityGroup.MapUserEndpoints();
        identityGroup.MapTenantEndpoints();
        identityGroup.MapEmailVerificationEndpoints();
        identityGroup.MapInvitationEndpoints();
        identityGroup.MapInvitationManagementEndpoints();
        identityGroup.MapSecurityEndpoints();
        identityGroup.MapRolesEndpoints();

        return endpoints;
    }
}
