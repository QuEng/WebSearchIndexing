using Microsoft.AspNetCore.Components;
using MudBlazor;
using WebSearchIndexing.Domain.Entities;
using WebSearchIndexing.Domain.Repositories;

namespace WebSearchIndexing.Pages.Urls.Components;

public partial class RejectedUrlsTableComponent : Pages.Components.ComponentBase
{
    private const int ROWS_PER_PAGE = 10;
    private List<UrlRequest> _allUrls = [];
    private bool _isLoadingUrls;
    private int _currentPage = 1;
    private int _totalPages = 1;

    [Inject]
    private IUrlRequestRepository? UrlRequestRepository { get; set; }

    protected override async Task OnParametersSetAsync()
    {
        var requestsCount = await UrlRequestRepository!.GetRequestsCountAsync(UrlRequestStatus.Failed);
        _totalPages = (int)Math.Ceiling(requestsCount / (double)ROWS_PER_PAGE);
        await UpdateUrlsListAsync();
    }

    private async Task UpdateUrlsListAsync()
    {
        _isLoadingUrls = true;
        StateHasChanged();

        var requestsCount = await UrlRequestRepository!.GetRequestsCountAsync(requestStatus: UrlRequestStatus.Failed);
        _totalPages = (int)Math.Ceiling(requestsCount / (double)ROWS_PER_PAGE);
        _totalPages = _totalPages == 0 ? 1 : _totalPages;
        if (_currentPage > _totalPages) _currentPage = _totalPages;

        _allUrls = await UrlRequestRepository!.TakeRequestsAsync(ROWS_PER_PAGE, (_currentPage - 1) * ROWS_PER_PAGE, UrlRequestStatus.Failed);

        _isLoadingUrls = false;
        StateHasChanged();
    }

    private async Task ChangePageAsync(int page)
    {
        _currentPage = page;
        await UpdateUrlsListAsync();
    }

    private async Task RemoveItemAsync(UrlRequest item)
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