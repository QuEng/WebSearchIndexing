using Google.Apis.Auth.OAuth2;
using Google.Apis.Indexing.v3;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using MudBlazor;
using WebSearchIndexing.Modules.Catalog.Application.Commands.ServiceAccounts;

namespace WebSearchIndexing.Modules.Catalog.Ui.Pages.ServiceAccounts.Dialogs;

public partial class AddServiceAccountDialog : ComponentBase
{
    private uint _quotaLimitPerDay;
    private bool _isUploadedFile;
    private string _serviceAccountsPath = string.Empty;
    private string _credentialsJson = string.Empty;
    private string _projectId = string.Empty;

    [CascadingParameter]
    private MudDialogInstance? MudDialog { get; set; }

    private async Task HandleUploadingFileAsync(InputFileChangeEventArgs e)
    {
        _isUploadedFile = false;
        StateHasChanged();

        if (e.File.Name.Contains(".json") is false)
        {
            Snackbar!.Add("Please upload a json file", Severity.Error);
            return;
        }

        GoogleCredential credential;
        try
        {
            using (var reader = new StreamReader(e.File.OpenReadStream()))
            {
                _credentialsJson = await reader.ReadToEndAsync();
            }

            credential = GoogleCredential
                .FromJson(_credentialsJson)
                .CreateScoped(IndexingService.Scope.Indexing);
        }
        catch (Exception)
        {
            Snackbar!.Add("An error occurred while reading the file", Severity.Error);
            return;
        }

        _projectId = ((ServiceAccountCredential)credential.UnderlyingCredential).ProjectId;
        _isUploadedFile = true;
        StateHasChanged();

        Snackbar!.Add("File uploaded successfully", Severity.Success);

        TryDeleteTempFile();
    }

    private void Save()
    {
        if (_isUploadedFile is false)
        {
            Snackbar!.Add("Please upload the service account key file", Severity.Error);
            return;
        }

        // Create command for adding service account via API
        var command = new AddServiceAccountCommand(_projectId, _credentialsJson, _quotaLimitPerDay);
        TryDeleteTempFile();
        MudDialog!.Close(command);
    }

    private void Cancel()
    {
        TryDeleteTempFile();
        MudDialog!.Close(null);
    }

    private void TryDeleteTempFile()
    {
        if (string.IsNullOrWhiteSpace(_serviceAccountsPath))
        {
            return;
        }

        try
        {
            File.Delete(_serviceAccountsPath);
        }
        catch { }
    }
}
