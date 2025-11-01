using System.Collections.Generic;
using Microsoft.AspNetCore.Components.Routing;
using MudBlazor;

namespace WebSearchIndexing.BuildingBlocks.Web.Navigation;

public sealed class NavigationItem
{
    private readonly List<NavigationItem> _children = new();

    public NavigationItem(string title, string icon, string? href, NavLinkMatch match, int order)
    {
        Title = title;
        Icon = icon;
        Href = href;
        Match = match;
        Order = order;
    }

    public string Title { get; }

    public string Icon { get; }

    public string? Href { get; }

    public NavLinkMatch Match { get; }

    public int Order { get; }

    public IReadOnlyList<NavigationItem> Children => _children;

    public bool IsGroup => _children.Count > 0;

    internal void AddChild(NavigationItem child)
    {
        _children.Add(child);
    }

    internal void SortChildren()
    {
        _children.Sort((left, right) => left.Order.CompareTo(right.Order));
    }
}
