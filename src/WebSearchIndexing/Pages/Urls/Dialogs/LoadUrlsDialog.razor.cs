using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using MudBlazor;
using WebSearchIndexing.Domain.Entities;
using WebSearchIndexing.Domain.Repositories;

namespace WebSearchIndexing.Pages.Urls.Dialogs;

public partial class LoadUrlsDialog : ComponentBase
{
    private UrlLoadType _urlLoadType = UrlLoadType.TextField;
    private string _urls = string.Empty;
    private string[] _invalidUrls = [];
    private List<UrlRequest> _urlRequests = [];
    private bool _isSavingUrls = false;
    private const string _defaultDragClass = "relative rounded-lg border-2 border-dashed pa-4 mud-width-full mud-height-full z-10 d-flex flex-column justify-center align-center";
    private string _dragClass = _defaultDragClass;

    [CascadingParameter]
    private IMudDialogInstance? MudDialog { get; set; }

    [Parameter, EditorRequired]
    public UrlRequestType UrlRequestType { get; set; }

    [Inject]
    private IUrlRequestRepository? UrlRequestRepository { get; set; }

    private const int MAX_URLS = 400;

    private void ChangeUrlLoadType(UrlLoadType urlLoadType)
    {
        _urlLoadType = urlLoadType;
        _urls = string.Empty;
        _invalidUrls = [];
        _urlRequests = [];
    }

    private async Task SaveAsync()
    {
        if (_isSavingUrls) return;

        if (_urlRequests.Any() is false)
        {
            Snackbar!.Add("No links found", Severity.Error);
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

    private void Close() => MudDialog!.Close();

    private void PrepareUrls()
    {
        if (string.IsNullOrWhiteSpace(_urls) && _urlLoadType == UrlLoadType.TextField)
        {
            Snackbar!.Add("No links found", Severity.Error);
            return;
        }

        _invalidUrls = [];
        _urlRequests = [];

        var urls = _urls.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        if (urls.Length > MAX_URLS && _urlLoadType == UrlLoadType.TextField)
        {
            Snackbar!.Add("Too many links", Severity.Error);
            return;
        }

        _invalidUrls = [.. urls.Where(url => IsUrlValid(url) is false)];
        _urlRequests = urls.Where(IsUrlValid)
            .Select(url => new UrlRequest { Url = url, Type = UrlRequestType, AddedAt = DateTime.UtcNow })
            .ToList();
    }

    protected async Task HandleFileSelectionAsync(InputFileChangeEventArgs e)
    {
        ClearDragClass();

        if (e.File.Name.Contains(".txt"))
        {
            try
            {
                using (var reader = new StreamReader(e.File.OpenReadStream()))
                {
                    _urls = await reader.ReadToEndAsync();
                }
            }
            catch (Exception ex)
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
        => _dragClass = $"{_defaultDragClass} mud-border-primary";

    protected void ClearDragClass()
        => _dragClass = _defaultDragClass;

    private bool IsUrlValid(string url)
    {
        return Uri.TryCreate(url, UriKind.Absolute, out Uri uri)
               && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
    }

    private enum UrlLoadType
    {
        TextField,
        File
    }
}