using Google.Apis.Auth.OAuth2;
using Google.Apis.Indexing.v3;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using MudBlazor;
using WebSearchIndexing.Domain.Entities;

namespace WebSearchIndexing.Pages.Dialogs;

public partial class AddServiceAccountDialog : ComponentBase
{
    private ServiceAccount _serviceAccount = new();
    private bool _isUploadedFile;
    private string _serviceAccountsPath = string.Empty;

    [CascadingParameter]
    private MudDialogInstance? _mudDialog { get; set; }

    private async void HandleUploadingFile(InputFileChangeEventArgs e)
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
                _serviceAccount.Json = await reader.ReadToEndAsync();
            }
            credential = GoogleCredential.FromJson(_serviceAccount.Json)
                                         .CreateScoped(IndexingService.Scope.Indexing);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            Snackbar!.Add("An error occurred while reading the file", Severity.Error);
            return;
        }

        _serviceAccount.ProjectId = ((ServiceAccountCredential)credential.UnderlyingCredential).ProjectId;
        _isUploadedFile = true;
        StateHasChanged();

        Snackbar.Add("File uploaded successfully", Severity.Success);

        try
        {
            File.Delete(_serviceAccountsPath);
        }
        catch { }
    }

    private void Save()
    {
        if (_serviceAccount.QuotaLimitPerDay < 0) _serviceAccount.QuotaLimitPerDay = 0;
        if (_isUploadedFile is false)
        {
            Snackbar!.Add("Please upload the service account key file", Severity.Error);
            return;
        }
        Close(_serviceAccount);
    }

    private void Close(object? obj)
    {
        if (obj is not ServiceAccount && _isUploadedFile)
        {
            try
            {
                File.Delete(_serviceAccountsPath);
            }
            catch { }
        }

        _mudDialog!.Close(obj);
    }
}