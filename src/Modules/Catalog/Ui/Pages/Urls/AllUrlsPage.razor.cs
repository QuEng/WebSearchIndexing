using Microsoft.AspNetCore.Components;
using WebSearchIndexing.Modules.Catalog.Domain;

namespace WebSearchIndexing.Modules.Catalog.Ui.Pages.Urls;

public partial class AllUrlsPage : ComponentBase
{
    private int _activePanelIndex;
    private string _title = "All {0} type urls";
    private UrlItemType _selectedRequestType = UrlItemType.Updated;

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
                _selectedRequestType = UrlItemType.Updated;
                break;

            case "deleted":
                _activePanelIndex = 1;
                _selectedRequestType = UrlItemType.Deleted;
                break;

            default:
                _activePanelIndex = 0;
                _selectedRequestType = UrlItemType.Updated;
                break;
        }
    }

    private void NavigateTo(string routeValue) => NavigationManager!.NavigateTo($"/all-urls/{routeValue}");
}
