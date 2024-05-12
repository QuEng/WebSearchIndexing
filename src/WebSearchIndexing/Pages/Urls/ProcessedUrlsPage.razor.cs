using Microsoft.AspNetCore.Components;
using WebSearchIndexing.Domain.Entities;

namespace WebSearchIndexing.Pages.Urls;

public partial class ProcessedUrlsPage : ComponentBase
{
    private int _activePanelIndex = 0;
    private UrlRequestType _selectedRequestType = UrlRequestType.Updated;

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

            case "rejected":
                _activePanelIndex = 2;
                break;

            default:
                _activePanelIndex = 0;
                _selectedRequestType = UrlRequestType.Updated;
                break;
        }

        _activePanelIndex = RouteText switch
        {
            "updated" => 0,
            "deleted" => 1,
            "rejected" => 2,
            _ => 0
        };
    }

    private void NavigateTo(string routeValue) => NavigationManager!.NavigateTo($"/processed-urls/{routeValue}");
}