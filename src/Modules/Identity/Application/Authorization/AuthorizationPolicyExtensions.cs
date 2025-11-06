using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using WebSearchIndexing.Modules.Identity.Application.Authorization.Handlers;
using WebSearchIndexing.Modules.Identity.Application.Authorization.Policies;
using WebSearchIndexing.Modules.Identity.Domain.Constants;

namespace WebSearchIndexing.Modules.Identity.Application.Authorization;

/// <summary>
/// Extension methods for configuring authorization policies
/// </summary>
public static class AuthorizationPolicyExtensions
{
    /// <summary>
    /// Adds Identity module authorization policies and handlers to the service collection
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddIdentityAuthorizationPolicies(this IServiceCollection services)
    {
        // Register authorization handlers
        services.AddScoped<IAuthorizationHandler, GlobalPermissionHandler>();
        services.AddScoped<IAuthorizationHandler, TenantPermissionHandler>();
        services.AddScoped<IAuthorizationHandler, DomainPermissionHandler>();
        services.AddScoped<IAuthorizationHandler, ResourceAccessHandler>();
        services.AddScoped<IAuthorizationHandler, TenantRoleHandler>();

        return services;
    }
}
