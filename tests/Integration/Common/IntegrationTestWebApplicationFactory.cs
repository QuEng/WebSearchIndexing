using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace WebSearchIndexing.Tests.Integration.Common;

public class IntegrationTestWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override IHost CreateHost(IHostBuilder builder)
    {
        // Configure test-specific services here
        builder.ConfigureServices(services =>
        {
            // Override services for testing
            // Example: Replace database with in-memory version
        });

        return base.CreateHost(builder);
    }
}
