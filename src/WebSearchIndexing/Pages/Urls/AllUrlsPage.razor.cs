using Microsoft.AspNetCore.Components;
using WebSearchIndexing.Domain.Entities;

namespace WebSearchIndexing.Pages.Urls;

public partial class AllUrlsPage : ComponentBase
{
    private int _activePanelIndex = 0;
    private string _title = "All {0} type urls";
    private UrlRequestType _selectedRequestType = UrlRequestType.Updated;

    [Parameter]
    public string? RouteText { get; set; }

    [Inject]
    private NavigationManager? NavigationManager { get; set; }

    protected override void OnParametersSet()
    {
        if (RouteText is null)
        {
            RouteText = "updated";
            NavigateTo(RouteText);
            return;
        }

        switch (RouteText)
        {
            case "updated":
                _activePanelIndex = 0;
                _selectedRequestType = UrlRequestType.Updated;
                break;

            case "deleted":
                _activePanelIndex = 1;
                _selectedRequestType = UrlRequestType.Deleted;
                break;

            default:
                _activePanelIndex = 0;
                _selectedRequestType = UrlRequestType.Updated;
                break;
        }
    }

    private void NavigateTo(string routeValue) => NavigationManager!.NavigateTo($"/all-urls/{routeValue}");
}