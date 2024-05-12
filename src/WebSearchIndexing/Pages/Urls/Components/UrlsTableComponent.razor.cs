using Microsoft.AspNetCore.Components;
using MudBlazor;
using WebSearchIndexing.Data.Repositories;
using WebSearchIndexing.Domain.Entities;
using WebSearchIndexing.Domain.Repositories;
using WebSearchIndexing.Pages.Urls.Dialogs;

namespace WebSearchIndexing.Pages.Urls.Components;

public partial class UrlsTableComponent : Pages.Components.ComponentBase
{
    private const int ROWS_PER_PAGE = 10;
    private List<UrlRequestChecked> _selectedUrls = [];
    private List<UrlRequestChecked> _filteredUrls = [];
    private List<UrlRequest> _allUrls = [];
    private bool _isClickedCheckedAll;
    private bool _isEditMode;
    private UrlRequestChecked _backupItem;
    private bool _isLoadingUrls;
    private bool _isHideCompleted;
    private int _currentPage = 1;
    private int _totalPages = 1;

    [Parameter, EditorRequired]
    public UrlRequestType UrlRequestType { get; set; }

    [Inject]
    private IDialogService? DialogService { get; set; }

    [Inject]
    private IUrlRequestRepository? UrlRequestRepository { get; set; }

    private List<UrlRequestChecked> SelectedUrls => _filteredUrls.Where(item => item.Checked).ToList();

    protected override async void OnAfterRender(bool firstRender)
    {
        if (!firstRender) return;

        await UpdateUrlsListAsync();
    }

    private async Task UpdateUrlsListAsync()
    {
        _isLoadingUrls = true;
        _filteredUrls = [];
        StateHasChanged();

        var requestsCount = await UrlRequestRepository!.GetRequestsCountAsync(requestStatus: _isHideCompleted ? UrlRequestStatus.Pending : null, requestType: UrlRequestType);
        _totalPages = (int)Math.Ceiling(requestsCount / (double)ROWS_PER_PAGE);
        _totalPages = _totalPages == 0 ? 1 : _totalPages;
        if (_currentPage > _totalPages) _currentPage = _totalPages;

        _allUrls = await UrlRequestRepository!.TakeRequestsAsync(ROWS_PER_PAGE, (_currentPage - 1) * ROWS_PER_PAGE, requestStatus: _isHideCompleted ? UrlRequestStatus.Pending : null, requestType: UrlRequestType);
        _filteredUrls = _allUrls.Select(item => new UrlRequestChecked() { UrlRequest = item, Checked = false }).ToList();

        _isLoadingUrls = false;
        StateHasChanged();
    }

    private async void ChangePage(int page)
    {
        _currentPage = page;
        await UpdateUrlsListAsync();
    }

    private async void ShowLoadLinksDialog()
    {
        var parameters = new DialogParameters()
        {
            { nameof(LoadUrlsDialog.UrlRequestType), UrlRequestType }
        };
        var dialog = await DialogService!.ShowAsync<LoadUrlsDialog>($"Load {UrlRequestType} links", parameters: parameters);
        var result = await dialog.Result;
        if (result.Data is bool isLoaded && isLoaded)
        {
            await UpdateUrlsListAsync();
        }
    }

    private void SelectAllLinks()
    {
        _isClickedCheckedAll = !_isClickedCheckedAll;
        _filteredUrls.ForEach(item => item.Checked = _isClickedCheckedAll);
    }

    private async void DeleteSelectedLinks()
    {
        var dialogResponse = await DialogService!.ShowMessageBox("Delete urls",
                                                                 $"You really want to delete the selected urls? ({SelectedUrls.Count()} pc.)",
                                                                 yesText: "Yes",
                                                                 cancelText: "No");

        if (dialogResponse is true)
        {
            await UrlRequestRepository!.RemoveRangeAsync(SelectedUrls.Select(item => item.UrlRequest));
            await UpdateUrlsListAsync();
            Snackbar!.Add("Selected links were successfully removed", Severity.Success);
        }
    }

    private async void ShowOrHideCompletedLinks()
    {
        _isHideCompleted = !_isHideCompleted;
        await UpdateUrlsListAsync();
    }

    private void OnRowClick(TableRowClickEventArgs<UrlRequestChecked> args)
    {
        if (_isEditMode) return;
        args.Item.Checked = !args.Item.Checked;
    }

    private void BackupItem(UrlRequestChecked item)
    {
        if (_isEditMode)
        {
            Snackbar!.Add("Please save the changes before editing another item", Severity.Error);
            return;
        }

        _isEditMode = true;
        item.Edited = true;
        item.Checked = false;

        _backupItem = new()
        {
            UrlRequest = new()
            {
                Url = item.UrlRequest.Url,
                Priority = item.UrlRequest.Priority
            }
        };
    }

    private void ResetItemToOriginalValue(UrlRequestChecked item)
    {
        _isEditMode = false;
        item.Edited = false;

        item.UrlRequest.Url = _backupItem.UrlRequest.Url;
        item.UrlRequest.Priority = _backupItem.UrlRequest.Priority;
    }

    private async void ItemHasBeenCommitted(UrlRequestChecked item)
    {
        if (IsUrlValid(item.UrlRequest.Url) is false)
        {
            Snackbar!.Add("The link is not valid", Severity.Error);
            return;
        }

        _isEditMode = false;
        item.Edited = false;

        if (await UrlRequestRepository!.UpdateAsync(item.UrlRequest))
        {
            Snackbar!.Add("Changes are successfully saved", Severity.Success);
        }
        else
        {
            Snackbar!.Add("An error occurred while saving changes. Please try again.", Severity.Error);
            ResetItemToOriginalValue(item);
            return;
        }
    }

    private async void RemoveItem(UrlRequestChecked item)
    {
        if (item.Edited)
        {
            _isEditMode = false;
        }
        if (await UrlRequestRepository!.DeleteAsync(item.UrlRequest.Id))
        {
            Snackbar!.Add("The link was successfully removed", Severity.Success);
        }
        else
        {
            Snackbar!.Add("An error occurred while removing the link. Please try again.", Severity.Error);
        }
    }

    private bool IsUrlValid(string url)
    {
        Uri uri;
        return Uri.TryCreate(url, UriKind.Absolute, out uri)
               && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
    }

    private class UrlRequestChecked
    {
        public bool Checked { get; set; }
        public bool Edited { get; set; }
        public UrlRequest UrlRequest { get; init; } = null!;
    }
}