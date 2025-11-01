using Microsoft.AspNetCore.Components;
using WebSearchIndexing.Modules.Catalog.Domain;

namespace WebSearchIndexing.Modules.Catalog.Ui.Pages.Urls;

public partial class ProcessedUrlsPage : ComponentBase
{
    private int _activePanelIndex;
    private UrlItemType _selectedRequestType = UrlItemType.Updated;

    [Parameter]
    public string? RouteText { get; set; }

    [Inject]
    public NavigationManager? NavigationManager { get; set; }

    protected override void OnParametersSet()
    {
        if (RouteText is null)
        {
            RouteText = "updated";
            NavigateTo(RouteText);
        }

        _activePanelIndex = RouteText switch
        {
            "updated" => 0,
            "deleted" => 1,
            "rejected" => 2,
            _ => 0
        };

        _selectedRequestType = RouteText switch
        {
            "deleted" => UrlItemType.Deleted,
            _ => UrlItemType.Updated
        };
    }

    private void NavigateTo(string routeValue) => NavigationManager!.NavigateTo($"/processed-urls/{routeValue}");
}
