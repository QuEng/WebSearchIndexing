using MudBlazor;
using WebSearchIndexing.BuildingBlocks.Web.Navigation;

namespace WebSearchIndexing.Hosts.WebHost.Navigation;

internal sealed class WebHostNavigationContributor : INavigationContributor
{
    public void Configure(NavigationBuilder builder)
    {
        builder.AddLink("Dashboard", Icons.Material.Filled.Dashboard, "/", NavLinkMatch.All, order: 0);
    }
}
