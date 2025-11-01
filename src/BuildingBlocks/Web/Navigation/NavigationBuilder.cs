using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Components.Routing;
using MudBlazor;

namespace WebSearchIndexing.BuildingBlocks.Web.Navigation;

public sealed class NavigationBuilder
{
    private readonly List<NavigationItem> _items = new();

    public NavigationBuilder AddLink(string title, string icon, string href, NavLinkMatch match = NavLinkMatch.Prefix, int order = 0)
    {
        _items.Add(new NavigationItem(title, icon, href, match, order));
        return this;
    }

    public NavigationGroupBuilder AddGroup(string title, string icon, int order = 0)
    {
        var groupItem = new NavigationItem(title, icon, href: null, match: NavLinkMatch.Prefix, order);
        _items.Add(groupItem);
        return new NavigationGroupBuilder(groupItem);
    }

    public IReadOnlyList<NavigationItem> Build()
    {
        return _items
            .OrderBy(item => item.Order)
            .Select(item =>
            {
                if (item.IsGroup)
                {
                    item.SortChildren();
                }

                return item;
            })
            .ToArray();
    }
}
