using Microsoft.AspNetCore.Components;
using MudBlazor;
using WebSearchIndexing.Modules.Catalog.Application.Abstractions;
using WebSearchIndexing.Modules.Catalog.Domain;

namespace WebSearchIndexing.Modules.Catalog.Ui.Pages.Urls.Components;

public partial class ProcessedUrlsTableComponent : WebSearchIndexing.BuildingBlocks.Web.Components.ComponentBase
{
    private const int RowsPerPage = 10;
    private List<UrlItem> _allUrls = [];
    private bool _isLoadingUrls = true;
    private int _currentPage = 1;
    private int _totalPages = 1;

    [Parameter, EditorRequired]
    public UrlItemType UrlRequestType { get; set; }

    [Inject]
    private IUrlRequestRepository? UrlRequestRepository { get; set; }

    protected override async Task OnParametersSetAsync()
    {
        await UpdateUrlsListAsync();
    }

    private async Task UpdateUrlsListAsync()
    {
        _isLoadingUrls = true;
        StateHasChanged();

        var requestsCount = await UrlRequestRepository!.GetRequestsCountAsync(
            requestStatus: UrlItemStatus.Completed,
            requestType: UrlRequestType);

        _totalPages = Math.Max(1, (int)Math.Ceiling(requestsCount / (double)RowsPerPage));
        if (_currentPage > _totalPages) _currentPage = _totalPages;

        _allUrls = await UrlRequestRepository.TakeRequestsAsync(
            RowsPerPage,
            (_currentPage - 1) * RowsPerPage,
            requestType: UrlRequestType);

        _isLoadingUrls = false;
        StateHasChanged();
    }

    private async Task ChangePageAsync(int page)
    {
        _currentPage = page;
        await UpdateUrlsListAsync();
    }

    private async Task RemoveItemAsync(UrlItem item)
    {
        if (await UrlRequestRepository!.DeleteAsync(item.Id))
        {
            Snackbar!.Add("The link was successfully removed", Severity.Success);
        }
        else
        {
            Snackbar!.Add("An error occurred while removing the link. Please try again.", Severity.Error);
        }
    }
}
