using Microsoft.AspNetCore.Components;
using MudBlazor;
using WebSearchIndexing.Contracts.Catalog;
using WebSearchIndexing.Modules.Catalog.Application.Commands.ServiceAccounts;
using WebSearchIndexing.Modules.Catalog.Ui.Pages.ServiceAccounts.Dialogs;
using WebSearchIndexing.Modules.Core.Ui.Contracts;
using WebSearchIndexing.Modules.Core.Ui.Models;

namespace WebSearchIndexing.Modules.Catalog.Ui.Pages.ServiceAccounts;

public partial class ServiceAccountsPage : ComponentBase
{
    private List<ServiceAccountDto> _serviceAccounts = [];
    private Dictionary<Guid, uint> _editingQuotas = new();
    private Guid _serviceAccountIdBeforeEdit;
    private uint _serviceAccountQuotaBeforeEdit;
    private bool _isLoading = true;

    [Inject]
    private IServiceAccountsApiService? ServiceAccountsApiService { get; set; }

    [Inject]
    private ICoreApiService? CoreApiService { get; set; }

    [Inject]
    private IDialogService? DialogService { get; set; }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender)
        {
            return;
        }

        await LoadDataAsync();
    }

    private async Task LoadDataAsync()
    {
        _isLoading = true;
        var serviceAccounts = await ServiceAccountsApiService!.GetAllAsync();
        _serviceAccounts = serviceAccounts?.ToList() ?? [];
        _isLoading = false;
        StateHasChanged();
    }

    private async Task ShowAddingDialogAsync()
    {
        var dialog = await DialogService!.ShowAsync<AddServiceAccountDialog>(
            "Add service account",
            new DialogOptions { CloseButton = true, FullWidth = true });

        var result = await dialog.Result;

        if (result.Data is AddServiceAccountCommand command)
        {
            if (await ServiceAccountsApiService!.ExistsAsync(command.ProjectId))
            {
                Snackbar!.Add("Service account with this project ID already exists", Severity.Error);
                return;
            }

            var request = new AddServiceAccountRequest(command.ProjectId, command.CredentialsJson, command.QuotaLimitPerDay);
            var addedServiceAccount = await ServiceAccountsApiService.AddAsync(request);
            if (addedServiceAccount is not null)
            {
                _serviceAccounts.Add(addedServiceAccount);
                _serviceAccounts = _serviceAccounts.OrderByDescending(x => x.CreatedAt).ToList();
                StateHasChanged();
                Snackbar!.Add("Service account added", Severity.Success);
            }
        }
    }

    private void BackupItem(object item)
    {
        var serviceAccount = (ServiceAccountDto)item;
        _serviceAccountIdBeforeEdit = serviceAccount.Id;
        _serviceAccountQuotaBeforeEdit = serviceAccount.QuotaLimitPerDay;
        _editingQuotas[serviceAccount.Id] = serviceAccount.QuotaLimitPerDay;
    }

    private void ResetItemToOriginalValues(object item)
    {
        var serviceAccount = (ServiceAccountDto)item;
        if (serviceAccount.Id != _serviceAccountIdBeforeEdit)
        {
            return;
        }

        _editingQuotas[serviceAccount.Id] = _serviceAccountQuotaBeforeEdit;
    }

    private void UpdateQuotaValue(ServiceAccountDto serviceAccount, uint newValue)
    {
        _editingQuotas[serviceAccount.Id] = newValue;
    }

    private uint GetQuotaForEditing(ServiceAccountDto serviceAccount)
    {
        return _editingQuotas.TryGetValue(serviceAccount.Id, out var value) ? value : serviceAccount.QuotaLimitPerDay;
    }

    private async Task ItemHasBeenCommittedAsync(object element)
    {
        var editedServiceAccount = (ServiceAccountDto)element;
        var newQuotaValue = GetQuotaForEditing(editedServiceAccount);

        if (newQuotaValue == _serviceAccountQuotaBeforeEdit)
        {
            return;
        }

        var request = new UpdateServiceAccountRequest(newQuotaValue);
        var updatedServiceAccount = await ServiceAccountsApiService!.UpdateAsync(editedServiceAccount.Id, request);
        if (updatedServiceAccount is not null)
        {
            // ???????? DTO ? ??????
            var index = _serviceAccounts.FindIndex(sa => sa.Id == editedServiceAccount.Id);
            if (index >= 0)
            {
                _serviceAccounts[index] = updatedServiceAccount;
                StateHasChanged();
            }
            Snackbar!.Add("Service account updated", Severity.Success);
        }
        else
        {
            Snackbar!.Add("Service account not found", Severity.Error);
            // ????????????? ?????? ??? ??????
            await LoadDataAsync();
        }

        // ???????? ????????? ??????
        _editingQuotas.Remove(editedServiceAccount.Id);
    }

    private async Task DeleteServiceAccountAsync(ServiceAccountDto serviceAccount)
    {
        var result = await DialogService!.ShowMessageBox(
            "Delete service account",
            "Are you sure you want to delete this service account?",
            yesText: "Delete",
            noText: "Cancel");

        if (result is null or false)
        {
            return;
        }

        try
        {
            await ServiceAccountsApiService!.DeleteAsync(serviceAccount.Id);
            _serviceAccounts.Remove(serviceAccount);
            StateHasChanged();
            Snackbar!.Add("Service account deleted", Severity.Success);

            if (_serviceAccounts.Count == 0)
            {
                await UpdateLimitAsync();
            }
        }
        catch
        {
            Snackbar!.Add("Failed to delete service account", Severity.Error);
        }
    }

    private async Task UpdateLimitAsync()
    {
        var setting = await CoreApiService!.GetSettingsAsync();
        if (setting is null)
        {
            return;
        }

        var sumAvailableLimit = _serviceAccounts.Sum(x => x.QuotaLimitPerDay);
        if (sumAvailableLimit < setting.RequestsPerDay)
        {
            var request = new UpdateSettingsRequest((int)sumAvailableLimit);
            var updatedSetting = await CoreApiService.UpdateSettingsAsync(request);
            if (updatedSetting is not null)
            {
                Snackbar!.Add("Requests per day updated", Severity.Success);
            }
        }
        else
        {
            Snackbar!.Add("Requests per day is already set to the maximum available value", Severity.Info);
        }
    }
}
