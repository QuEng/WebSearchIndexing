using Microsoft.AspNetCore.Components;
using MudBlazor;
using WebSearchIndexing.Domain.Entities;
using WebSearchIndexing.Domain.Repositories;
using WebSearchIndexing.Pages.Dialogs;

namespace WebSearchIndexing.Pages;

public partial class ServiceAccountsPage : ComponentBase
{
    private List<ServiceAccount> _serviceAccounts = [];
    private ServiceAccount _serviceAccountBeforeEdit = new();
    private bool _isLoading = true;

    [Inject]
    private IServiceAccountRepository? ServiceAccountRepository { get; set; }

    [Inject]
    private ISettingRepository? SettingRepository { get; set; }

    [Inject]
    private IDialogService? DialogService { get; set; }

    protected override async void OnAfterRender(bool firstRender)
    {
        if (!firstRender) return;

        _serviceAccounts = await ServiceAccountRepository!.GetAllAsync();
        _serviceAccounts = [.. _serviceAccounts.OrderByDescending(x => x.CreatedAt)];
        _isLoading = false;
        StateHasChanged();
    }

    private async void ShowAddingDialog()
    {
        var dialog = await DialogService!.ShowAsync<AddServiceAccountDialog>("Add service account",
            new DialogOptions() { CloseButton = true, FullWidth = true });

        var result = await dialog.Result;

        if (result.Data is ServiceAccount newServiceAccount)
        {
            if (await ServiceAccountRepository!.EntityExistByProjectIdAsync(newServiceAccount.ProjectId))
            {
                Snackbar!.Add("Service account with this project ID already exists", Severity.Error);
                return;
            }
            if ((newServiceAccount = await ServiceAccountRepository!.AddAsync(newServiceAccount!)) is not null)
            {
                _serviceAccounts.Add(newServiceAccount);
                _serviceAccounts = _serviceAccounts.OrderByDescending(x => x.CreatedAt).ToList();
                StateHasChanged();
                Snackbar!.Add("Service account added", Severity.Success);
            }
        }
    }

    private void BackupItem(object item)
    {
        _serviceAccountBeforeEdit = new()
        {
            Id = ((ServiceAccount)item).Id,
            QuotaLimitPerDay = ((ServiceAccount)item).QuotaLimitPerDay
        };
    }

    private void ResetItemToOriginalValues(object item)
    {
        ((ServiceAccount)item).Id = _serviceAccountBeforeEdit.Id;
        ((ServiceAccount)item).QuotaLimitPerDay = _serviceAccountBeforeEdit.QuotaLimitPerDay;
    }

    private async void ItemHasBeenCommitted(object element)
    {
        var newQuotaValue = ((ServiceAccount)element).QuotaLimitPerDay;

        if (newQuotaValue == _serviceAccountBeforeEdit.QuotaLimitPerDay) return;

        var serviceAccount = await ServiceAccountRepository!.GetByIdAsync(((ServiceAccount)element).Id);

        if (serviceAccount is null)
        {
            Snackbar!.Add("Service account not found", Severity.Error);
            return;
        }

        serviceAccount.QuotaLimitPerDay = newQuotaValue;
        if (await ServiceAccountRepository!.UpdateAsync(serviceAccount))
        {
            Snackbar!.Add("Service account updated", Severity.Success);
        }
    }

    private async void DeleteServiceAccount(ServiceAccount serviceAccount)
    {
        var result = await DialogService!.ShowMessageBox("Delete service account",
            "Are you sure you want to delete this service account?",
            yesText: "Delete",
            noText: "Cancel");

        if (result is null || result is false) return;

        if (await ServiceAccountRepository!.DeleteAsync(serviceAccount.Id))
        {
            _serviceAccounts.Remove(serviceAccount);
            StateHasChanged();
            Snackbar!.Add("Service account deleted", Severity.Success);

            if (_serviceAccounts.Count == 0)
            {
                UpdateLimit();
            }
        }
    }

    private async void UpdateLimit()
    {
        var setting = await SettingRepository!.GetSettingAsync();
        if (setting is null) return;
        var sumAvailableLimit = _serviceAccounts.Sum(x => x.QuotaLimitPerDay);
        if (sumAvailableLimit < setting.RequestsPerDay)
        {
            setting.RequestsPerDay = (int)sumAvailableLimit;
            await SettingRepository!.UpdateAsync(setting);
            Snackbar!.Add("Requests per day updated", Severity.Success);
        }
        else
        {
            Snackbar!.Add("Requests per day is already set to the maximum available value", Severity.Info);
        }
    }
}