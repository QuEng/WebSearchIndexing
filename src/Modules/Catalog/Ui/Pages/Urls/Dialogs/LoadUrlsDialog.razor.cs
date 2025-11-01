using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using MudBlazor;
using WebSearchIndexing.Domain.Repositories;
using WebSearchIndexing.Modules.Catalog.Domain;

namespace WebSearchIndexing.Modules.Catalog.Ui.Pages.Urls.Dialogs;

public partial class LoadUrlsDialog : ComponentBase
{
    private UrlLoadType _urlLoadType = UrlLoadType.TextField;
    private string _urls = string.Empty;
    private string[] _invalidUrls = [];
    private List<UrlItem> _urlRequests = [];
    private bool _isSavingUrls;
    private const string DefaultDragClass = "relative rounded-lg border-2 border-dashed pa-4 mud-width-full mud-height-full z-10 d-flex flex-column justify-center align-center";
    private string _dragClass = DefaultDragClass;

    [CascadingParameter]
    private MudDialogInstance? MudDialog { get; set; }

    [Parameter, EditorRequired]
    public UrlItemType UrlRequestType { get; set; }

    [Inject]
    private IUrlRequestRepository? UrlRequestRepository { get; set; }

    private const int MaxUrls = 400;

    private void ChangeUrlLoadType(UrlLoadType urlLoadType)
    {
        _urlLoadType = urlLoadType;
        _urls = string.Empty;
        _invalidUrls = [];
        _urlRequests = [];
    }

    private async Task SaveAsync()
    {
        if (_isSavingUrls || _urlRequests.Any() is false)
        {
            if (_urlRequests.Any() is false)
            {
                Snackbar!.Add("No links found", Severity.Error);
            }
            return;
        }

        _isSavingUrls = true;

        if (await UrlRequestRepository!.AddRangeAsync(_urlRequests))
        {
            Snackbar!.Add("Links added", Severity.Success);
            MudDialog!.Close(true);
        }
        else
        {
            _isSavingUrls = false;
            Snackbar!.Add("Failed to add links", Severity.Error);
        }
    }

    private void Close()
    {
        MudDialog!.Close();
    }

    private void PrepareUrls()
    {
        if (string.IsNullOrWhiteSpace(_urls) && _urlLoadType == UrlLoadType.TextField)
        {
            Snackbar!.Add("No links found", Severity.Error);
            return;
        }

        _invalidUrls = [];
        _urlRequests = [];

        var urls = _urls.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (urls.Length > MaxUrls && _urlLoadType == UrlLoadType.TextField)
        {
            Snackbar!.Add("Too many links", Severity.Error);
            return;
        }

        _invalidUrls = [.. urls.Where(url => IsUrlValid(url) is false)];
        _urlRequests = urls
            .Where(IsUrlValid)
            .Select(url => new UrlItem(url, UrlRequestType, UrlItemPriority.Medium))
            .ToList();
    }

    protected async Task HandleFileSelectionAsync(InputFileChangeEventArgs e)
    {
        ClearDragClass();

        if (e.File.Name.Contains(".txt"))
        {
            try
            {
                using var reader = new StreamReader(e.File.OpenReadStream());
                _urls = await reader.ReadToEndAsync();
            }
            catch
            {
                Snackbar!.Add("An error occurred while uploading a file", Severity.Error);
                return;
            }
        }
        else
        {
            Snackbar!.Add("Only a txt file is supported", Severity.Warning);
            return;
        }

        PrepareUrls();
    }

    protected void SetDragClass()
        => _dragClass = $"{DefaultDragClass} mud-border-primary";

    protected void ClearDragClass()
        => _dragClass = DefaultDragClass;

    private static bool IsUrlValid(string url)
    {
        return Uri.TryCreate(url, UriKind.Absolute, out var uri) &&
               (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
    }

    private enum UrlLoadType
    {
        TextField,
        File
    }
}
