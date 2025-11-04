using Microsoft.AspNetCore.Components;
using MudBlazor;
using WebSearchIndexing.Modules.Catalog.Domain.Entities;
using WebSearchIndexing.Modules.Catalog.Ui.Pages.Urls.Dialogs;

namespace WebSearchIndexing.Modules.Catalog.Ui.Pages.Batches;

public partial class BatchesPage : ComponentBase
{
    private UrlItemType _selectedType = UrlItemType.Updated;

    [Inject]
    private IDialogService DialogService { get; set; } = default!;

    private async Task OpenImportDialogAsync()
    {
        var parameters = new DialogParameters
        {
            { nameof(LoadUrlsDialog.UrlRequestType), _selectedType }
        };

        var dialog = await DialogService.ShowAsync<LoadUrlsDialog>(
            $"Import {_selectedType.ToString().ToLower()} urls",
            parameters,
            new DialogOptions { CloseButton = true, FullWidth = true });

        var result = await dialog.Result;
        if (result.Data is true)
        {
            Snackbar!.Add("Urls queued for processing", Severity.Success);
        }
    }
}
