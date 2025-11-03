using Microsoft.AspNetCore.Components;
using MudBlazor;
using WebSearchIndexing.Modules.Catalog.Application.DTOs;
using WebSearchIndexing.Modules.Catalog.Domain;
using WebSearchIndexing.Modules.Catalog.Ui.Pages.Urls.Dialogs;
using WebSearchIndexing.Modules.Catalog.Ui.Services;

namespace WebSearchIndexing.Modules.Catalog.Ui.Pages.Urls.Components;

public partial class UrlsTableComponent : WebSearchIndexing.BuildingBlocks.Web.Components.ComponentBase
{
    private const int RowsPerPage = 10;
    private readonly List<UrlEntry> _selectedUrls = [];
    private List<UrlEntry> _filteredUrls = [];
    private List<UrlItemDto> _allUrls = [];
    private bool _isClickedCheckedAll;
    private bool _isEditMode;
    private Guid _editingUrlId;
    private string _backupUrl = string.Empty;
    private UrlItemPriority _backupPriority;
    private bool _isLoadingUrls;
    private bool _isHideCompleted;
    private int _currentPage = 1;
    private int _totalPages = 1;

    [Parameter, EditorRequired]
    public UrlItemType UrlRequestType { get; set; }

    [Inject]
    private IDialogService? DialogService { get; set; }

    [Inject]
    private IUrlsApiService? UrlsApiService { get; set; }

    private List<UrlEntry> SelectedUrls => _filteredUrls.Where(item => item.Checked).ToList();

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender) return;

        await UpdateUrlsListAsync();
    }

    private async Task UpdateUrlsListAsync()
    {
        _isLoadingUrls = true;
        _filteredUrls = [];
        StateHasChanged();

        var requestsCount = await UrlsApiService!.GetUrlsCountAsync(
            status: _isHideCompleted ? UrlItemStatus.Pending : null,
            type: UrlRequestType);

        _totalPages = Math.Max(1, (int)Math.Ceiling(requestsCount / (double)RowsPerPage));
        if (_currentPage > _totalPages) _currentPage = _totalPages;

        _allUrls = (await UrlsApiService.GetUrlsAsync(
            RowsPerPage,
            (_currentPage - 1) * RowsPerPage,
            status: _isHideCompleted ? UrlItemStatus.Pending : null,
            type: UrlRequestType)).ToList();

        _filteredUrls = _allUrls.Select(item => new UrlEntry(item)).ToList();

        _isLoadingUrls = false;
        StateHasChanged();
    }

    private async Task ChangePageAsync(int page)
    {
        _currentPage = page;
        await UpdateUrlsListAsync();
    }

    private async Task ShowLoadLinksDialogAsync()
    {
        var parameters = new DialogParameters
        {
            { nameof(LoadUrlsDialog.UrlRequestType), UrlRequestType }
        };
        var dialog = await DialogService!.ShowAsync<LoadUrlsDialog>($"Load {UrlRequestType} links", parameters);
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

    private async Task DeleteSelectedLinksAsync()
    {
        var dialogResponse = await DialogService!.ShowMessageBox(
            "Delete urls",
            $"You really want to delete the selected urls? ({SelectedUrls.Count} pc.)",
            yesText: "Yes",
            cancelText: "No");

        if (dialogResponse is true)
        {
            var urlIds = SelectedUrls.Select(item => item.UrlRequest.Id).ToList();
            var success = await UrlsApiService!.DeleteUrlsBatchAsync(urlIds);
            
            if (success)
            {
                await UpdateUrlsListAsync();
                Snackbar!.Add("Selected links were successfully removed", Severity.Success);
            }
            else
            {
                Snackbar!.Add("An error occurred while removing the links. Please try again.", Severity.Error);
            }
        }
    }

    private async Task ShowOrHideCompletedLinksAsync()
    {
        _isHideCompleted = !_isHideCompleted;
        await UpdateUrlsListAsync();
    }

    private void OnRowClick(TableRowClickEventArgs<UrlEntry> args)
    {
        if (_isEditMode) return;
        args.Item.Checked = !args.Item.Checked;
    }

    private void BackupItem(UrlEntry item)
    {
        if (_isEditMode)
        {
            Snackbar!.Add("Please save the changes before editing another item", Severity.Error);
            return;
        }

        _isEditMode = true;
        item.Edited = true;
        item.Checked = false;

        _editingUrlId = item.UrlRequest.Id;
        _backupUrl = item.UrlRequest.Url;
        _backupPriority = item.UrlRequest.Priority;
    }

    private void ResetItemToOriginalValue(UrlEntry item)
    {
        _isEditMode = false;
        item.Edited = false;

        if (item.UrlRequest.Id != _editingUrlId)
        {
            return;
        }

        item.UrlRequest.UpdateUrl(_backupUrl);
        item.UrlRequest.UpdatePriority(_backupPriority);
    }

    private async Task ItemHasBeenCommittedAsync(UrlEntry item)
    {
        if (IsUrlValid(item.UrlRequest.Url) is false)
        {
            Snackbar!.Add("The link is not valid", Severity.Error);
            return;
        }

        _isEditMode = false;
        item.Edited = false;

        try
        {
            var updatedDto = await UrlsApiService!.UpdateUrlAsync(
                item.UrlRequest.Id,
                item.UrlRequest.Url,
                item.UrlRequest.Priority);

            // Update the item with the response from server
            item.UpdateFromDto(updatedDto);

            Snackbar!.Add("Changes are successfully saved", Severity.Success);
        }
        catch (Exception)
        {
            Snackbar!.Add("An error occurred while saving changes. Please try again.", Severity.Error);
            ResetItemToOriginalValue(item);
        }
    }

    private async Task RemoveItemAsync(UrlEntry item)
    {
        if (item.Edited)
        {
            _isEditMode = false;
        }
        
        var success = await UrlsApiService!.DeleteUrlAsync(item.UrlRequest.Id);
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

    private static bool IsUrlValid(string url)
    {
        return Uri.TryCreate(url, UriKind.Absolute, out var uri) &&
               (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
    }

    private class UrlEntry
    {
        public UrlEntry(UrlItemDto urlItem)
        {
            UrlRequest = new EditableUrlItem(urlItem);
            UrlItem = urlItem;
        }

        public bool Checked { get; set; }
        public bool Edited { get; set; }
        public EditableUrlItem UrlRequest { get; }
        public UrlItemDto UrlItem { get; private set; }

        public void UpdateFromDto(UrlItemDto dto)
        {
            UrlItem = dto;
        }
    }
}
