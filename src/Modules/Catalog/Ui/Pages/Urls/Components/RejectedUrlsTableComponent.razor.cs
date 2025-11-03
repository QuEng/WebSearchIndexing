using Microsoft.AspNetCore.Components;
using MudBlazor;
using WebSearchIndexing.Modules.Catalog.Application.DTOs;
using WebSearchIndexing.Modules.Catalog.Domain;
using WebSearchIndexing.Modules.Catalog.Ui.Services;

namespace WebSearchIndexing.Modules.Catalog.Ui.Pages.Urls.Components;

public partial class RejectedUrlsTableComponent : WebSearchIndexing.BuildingBlocks.Web.Components.ComponentBase
{
    private const int RowsPerPage = 10;
    private List<UrlItemDto> _allUrls = [];
    private bool _isLoadingUrls;
    private int _currentPage = 1;
    private int _totalPages = 1;

    [Inject]
    private IUrlsApiService? UrlsApiService { get; set; }

    protected override async Task OnParametersSetAsync()
    {
        var requestsCount = await UrlsApiService!.GetUrlsCountAsync(UrlItemStatus.Failed);
        _totalPages = Math.Max(1, (int)Math.Ceiling(requestsCount / (double)RowsPerPage));
        await UpdateUrlsListAsync();
    }

    private async Task UpdateUrlsListAsync()
    {
        _isLoadingUrls = true;
        StateHasChanged();

        var requestsCount = await UrlsApiService!.GetUrlsCountAsync(status: UrlItemStatus.Failed);
        _totalPages = Math.Max(1, (int)Math.Ceiling(requestsCount / (double)RowsPerPage));
        if (_currentPage > _totalPages) _currentPage = _totalPages;

        _allUrls = (await UrlsApiService.GetUrlsAsync(
            RowsPerPage,
            (_currentPage - 1) * RowsPerPage,
            UrlItemStatus.Failed)).ToList();

        _isLoadingUrls = false;
        StateHasChanged();
    }

    private async Task ChangePageAsync(int page)
    {
        _currentPage = page;
        await UpdateUrlsListAsync();
    }

    private async Task RemoveItemAsync(UrlItemDto item)
    {
        var success = await UrlsApiService!.DeleteUrlAsync(item.Id);
        if (success)
        {
            await UpdateUrlsListAsync();
            Snackbar!.Add("The link was successfully removed", Severity.Success);
        }
        else
        {
            Snackbar!.Add("An error occurred while removing the link. Please try again.", Severity.Error);
        }
    }
}
