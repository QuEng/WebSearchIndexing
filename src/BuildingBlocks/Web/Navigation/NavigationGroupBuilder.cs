using Microsoft.AspNetCore.Components.Routing;
using MudBlazor;

namespace WebSearchIndexing.BuildingBlocks.Web.Navigation;

public sealed class NavigationGroupBuilder
{
    private readonly NavigationItem _groupItem;

    internal NavigationGroupBuilder(NavigationItem groupItem)
    {
        _groupItem = groupItem;
    }

    public NavigationGroupBuilder AddLink(string title, string icon, string href, NavLinkMatch match = NavLinkMatch.Prefix, int order = 0)
    {
        _groupItem.AddChild(new NavigationItem(title, icon, href, match, order));
        return this;
    }

    internal NavigationItem Build()
    {
        _groupItem.SortChildren();
        return _groupItem;
    }
}
