using Microsoft.Extensions.DependencyInjection;
using WebSearchIndexing.BuildingBlocks.Messaging;
using WebSearchIndexing.Modules.Identity.Application.Abstractions;
using WebSearchIndexing.Modules.Identity.Application.Services;
using WebSearchIndexing.Modules.Identity.Application.Authorization;
using WebSearchIndexing.Modules.Identity.Application.Authorization.Commands.AssignRole;
using WebSearchIndexing.Modules.Identity.Application.UserManagement.Commands.InviteUser;
using WebSearchIndexing.Modules.Identity.Application.UserManagement.Queries.GetUsers;
using WebSearchIndexing.Modules.Identity.Application.IntegrationEventHandlers.Catalog;
using WebSearchIndexing.Modules.Identity.Application.IntegrationEventHandlers.Core;
using WebSearchIndexing.Modules.Identity.Application.IntegrationEvents.External;

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

        // Register Integration Event Handlers
        services.AddIntegrationEventHandler<ServiceAccountCreatedEvent, ServiceAccountCreatedEventHandler>();
        services.AddIntegrationEventHandler<ServiceAccountUpdatedEvent, ServiceAccountUpdatedEventHandler>();
        services.AddIntegrationEventHandler<ServiceAccountDeletedEvent, ServiceAccountDeletedEventHandler>();
        services.AddIntegrationEventHandler<SettingsChangedEvent, SettingsChangedEventHandler>();

        // TODO: Add MediatR handlers when they are implemented
        // TODO: Add validators

        return services;
    }
}
