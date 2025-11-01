using System;
using System.Collections.Generic;

namespace WebSearchIndexing.BuildingBlocks.Web.Navigation;

public interface INavigationService
{
    IReadOnlyList<NavigationItem> GetItems();
}

internal sealed class NavigationService : INavigationService
{
    private readonly Lazy<IReadOnlyList<NavigationItem>> _navigation;

    public NavigationService(IEnumerable<INavigationContributor> contributors)
    {
        _navigation = new Lazy<IReadOnlyList<NavigationItem>>(() =>
        {
            var builder = new NavigationBuilder();
            foreach (var contributor in contributors)
            {
                contributor.Configure(builder);
            }

            return builder.Build();
        });
    }

    public IReadOnlyList<NavigationItem> GetItems() => _navigation.Value;
}
