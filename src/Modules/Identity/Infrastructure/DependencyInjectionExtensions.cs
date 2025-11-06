using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql.EntityFrameworkCore.PostgreSQL;
using WebSearchIndexing.BuildingBlocks.Messaging;
using WebSearchIndexing.BuildingBlocks.Persistence;
using WebSearchIndexing.Modules.Identity.Application.Abstractions;
using WebSearchIndexing.Modules.Identity.Application.Security;
using WebSearchIndexing.Modules.Identity.Application.Services;
using WebSearchIndexing.Modules.Identity.Domain.Repositories;
using WebSearchIndexing.Modules.Identity.Infrastructure.Configuration;
using WebSearchIndexing.Modules.Identity.Infrastructure.Persistence;
using WebSearchIndexing.Modules.Identity.Infrastructure.Persistence.Repositories;
using WebSearchIndexing.Modules.Identity.Infrastructure.Services;

namespace WebSearchIndexing.Modules.Identity.Infrastructure;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddIdentityInfrastructureModule(
        this IServiceCollection services, 
        IConfiguration configuration)
    {
        return services.AddIdentityInfrastructure(configuration);
    }

    public static IServiceCollection AddIdentityInfrastructure(
        this IServiceCollection services, 
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        // Configure JWT Settings
        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));

        var connectionString = configuration.GetConnectionString("IndexingDb");

        // Database Context with pooled factory
        services.AddPooledDbContextFactory<IdentityDbContext>((sp, options) =>
        {
            options.UseNpgsql(connectionString, sqlOptions =>
            {
                sqlOptions.MigrationsAssembly(typeof(IdentityDbContext).Assembly.FullName);
                sqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", "identity");
                sqlOptions.ExecutionStrategy(c => new NpgsqlRetryingExecutionStrategy(c, 4));
            });
        });

        // Register IdentityDbContext as scoped for dependency injection
        services.AddScoped<IdentityDbContext>(sp =>
        {
            var factory = sp.GetRequiredService<IDbContextFactory<IdentityDbContext>>();
            return factory.CreateDbContext();
        });

        // Repositories
        services.AddScoped<Domain.Repositories.IUserRepository>(sp =>
        {
            var inner = sp.GetRequiredService<UserRepository>();
            var cache = sp.GetRequiredService<Application.Caching.ICacheService>();
            return new Caching.CachedUserRepository(inner, cache);
        });
        services.AddScoped<UserRepository>(); // Register concrete type for decorator

        services.AddScoped<Domain.Repositories.ITenantRepository, TenantRepository>();
        
        services.AddScoped<Domain.Repositories.IRoleRepository>(sp =>
        {
            var inner = sp.GetRequiredService<Persistence.Repositories.RoleRepository>();
            var cache = sp.GetRequiredService<Application.Caching.ICacheService>();
            return new Caching.CachedRoleRepository(inner, cache);
        });
        services.AddScoped<Persistence.Repositories.RoleRepository>(); // Register concrete type for decorator

        services.AddScoped<Domain.Repositories.IUserInvitationRepository, UserInvitationRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<IEmailVerificationRepository, EmailVerificationRepository>();

        // Caching
        services.AddSingleton<Application.Caching.ICacheService, Caching.InMemoryCacheService>();

        // Services
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IIdentityEmailService, IdentityEmailService>();
        services.AddScoped<IEmailVerificationService, EmailVerificationService>();
        services.AddScoped<IPasswordPolicyService, Security.PasswordPolicyService>();
        services.AddScoped<Domain.Services.IAuthorizationDomainService, Services.AuthorizationDomainService>();

        // Configure Password Policy
        services.Configure<PasswordPolicyOptions>(configuration.GetSection("Identity:PasswordPolicy"));

        // Configure Cookie Security
        services.Configure<CookieSecurityOptions>(configuration.GetSection("Identity:CookieSecurity"));
        services.AddSingleton<ICookieSecurityValidator, Security.CookieSecurityValidator>();

        // Security Services
        var rateLimitOptions = new Security.RateLimitOptions();
        configuration.GetSection("Identity:RateLimiting").Bind(rateLimitOptions);
        services.AddSingleton(rateLimitOptions);
        services.AddSingleton<Security.IRateLimitService>(sp => 
            new Security.InMemoryRateLimitService(sp.GetRequiredService<Security.RateLimitOptions>()));

        var lockoutOptions = new Security.AccountLockoutOptions();
        configuration.GetSection("Identity:AccountLockout").Bind(lockoutOptions);
        services.AddSingleton(lockoutOptions);
        services.AddSingleton<ISecurityLoggingService, Security.SecurityLoggingService>();
        services.AddSingleton<ITokenSecurityValidator, Security.InMemoryTokenSecurityValidator>();
        services.AddSingleton<Security.IAccountLockoutService>(sp => 
            new Security.InMemoryAccountLockoutService(
                sp.GetRequiredService<Security.AccountLockoutOptions>(),
                sp.GetRequiredService<ISecurityLoggingService>()));

        // Add messaging and outbox support
        services.AddMessaging();
        services.AddOutboxRepository<IdentityDbContext>();

        return services;
    }
}
