using Microsoft.Extensions.DependencyInjection;
using WebSearchIndexing.Modules.Identity.Application.Abstractions;
using WebSearchIndexing.Modules.Identity.Application.Services;
using WebSearchIndexing.Modules.Identity.Application.Authorization;
using WebSearchIndexing.Modules.Identity.Application.Authorization.Commands.AssignRole;
using WebSearchIndexing.Modules.Identity.Application.UserManagement.Commands.InviteUser;
using WebSearchIndexing.Modules.Identity.Application.UserManagement.Queries.GetUsers;

namespace WebSearchIndexing.Modules.Identity.Application;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddIdentityApplicationModule(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        // Register application services
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ITenantService, TenantService>();
        services.AddScoped<IInvitationService, InvitationService>();
        services.AddScoped<IInvitationManagerService, InvitationManagerService>();

        // Register command handlers
        services.AddScoped<AssignRoleHandler>();
        services.AddScoped<InviteUserHandler>();

        // Register query handlers
        services.AddScoped<GetUsersHandler>();

        // Register authorization policies
        services.AddIdentityAuthorizationPolicies();

        // TODO: Add MediatR handlers when they are implemented
        // TODO: Add validators

        return services;
    }
}
